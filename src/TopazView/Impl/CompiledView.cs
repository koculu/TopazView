using System.Text;
using Tenray.TopazView.DI;
using Tenray.TopazView.Exceptions;
using Tenray.TopazView.Extensions;

namespace Tenray.TopazView.Impl;

internal sealed class CompiledView : ICompiledViewInternal, IDisposable
{
    public string Path => ViewInternal.Path;

    public string ViewContent { get; set; }

    public bool IsCompiled { get; set; }

    IViewEngineComponents ViewEngineComponents { get; }

    public IJavascriptEngine JavascriptEngine { get; set; }

    public View ViewInternal { get; }

    public string Layout { get; set; }

    public Dictionary<string, ICompiledViewInternal> Sections;

    public Dictionary<string, ICompiledViewInternal> ScriptSections;

    public List<TextPart> TextParts { get; set; }

    readonly internal object ViewCompilationLockObject = new();

    readonly object _contentRetrievalLockObject = new();

    Task<string> _contentRetrievalTask;

    public CompiledView(
        IViewEngineComponents viewEngineComponents,
        IJavascriptEngineProvider javascriptEngineProvider,
        View viewInternal)
    {
        ViewEngineComponents = viewEngineComponents;
        ViewInternal = viewInternal;
        JavascriptEngine =
            viewInternal.Flags.HasFlag(ViewFlags.PrivateJavascriptEngine)
            ? javascriptEngineProvider.GetJavascriptEngine()
            : viewEngineComponents.GlobalJavascriptEngine;
    }

    public async Task<string> RetrieveContent()
    {
        if (ViewContent != null)
            return ViewContent;

        if (_contentRetrievalTask != null)
        {
            ViewContent = await _contentRetrievalTask.ConfigureAwait(false);
            return ViewContent;
        }

        lock (_contentRetrievalLockObject)
        {
            if (_contentRetrievalTask == null)
            {
                _contentRetrievalTask =
                    ViewEngineComponents
                    .ContentProvider
                    .GetContentAsync(Path)
                    .AsTask();
            }
        }
        ViewContent = await _contentRetrievalTask.ConfigureAwait(false);
        return ViewContent;
    }

    public bool RunScriptSection(IViewRenderContext context, string scriptName)
    {
        if (!ScriptSections.TryGetValue(scriptName, out var scriptSection))
            return false;

        scriptSection.RenderViewNoLayout(context);
        return true;
    }

    public bool RenderSection(IViewRenderContext context, string sectionName)
    {
        if (!Sections.TryGetValue(sectionName, out var section))
            return false;

        section.RenderViewNoLayout(context);
        return true;
    }

    public async ValueTask RenderView(IViewRenderContext context)
    {
        var contextInternal = (IViewRenderContextInternal)context;
        contextInternal.ViewFlags = ViewInternal.Flags;
        RunScriptSection(contextInternal, "onLoad");
        var layout = contextInternal.Page.layout ?? Layout;
        if (layout != null)
        {
            var absoluteLayout = Path.JoinPath("../", layout);
            var layoutView =
                await ViewEngineComponents.ViewRepository
                .GetOrCreateView(absoluteLayout, ViewInternal.Flags)
                .GetCompiledViewAsync()
                .ConfigureAwait(false);
            contextInternal.Layout = (ICompiledViewInternal)layoutView;
            contextInternal.Body = this;
            layoutView.RenderViewNoLayout(contextInternal);
            return;
        }
        RenderViewNoLayout(contextInternal);
    }

    public void RenderViewNoLayout(IViewRenderContext context)
    {
        try
        {
            RenderViewNoLayoutInternal(context);
        }
        catch (Exception e)
        {
            throw new RenderException($@"View rendering failed.
View: {Path}", e);
        }
    }

    void RenderViewNoLayoutInternal(IViewRenderContext context)
    {
        var contextInternal = (IViewRenderContextInternal)context;
        contextInternal.ViewFlags = ViewInternal.Flags;
        var existingRenderingNow = contextInternal.RenderingNow;
        var existingRenderingPath = contextInternal.RenderingPath;
        if (contextInternal.IsRenderingPartNow(Path))
        {
#if DEBUG
            throw new ViewCompilerException($"Recursive rendering detected: {Path}");
#else
            return;
#endif
        }

        contextInternal.MarkPartRenderingNow(Path);
        var encoding = contextInternal.Encoding;
        contextInternal.RenderingNow = this;
        contextInternal.RenderingPath = Path;
        var viewContent = ViewContent.AsSpan();
        var list = TextParts;
        var jsEngine = JavascriptEngine;
        var lastIfStatementCondition = false;
        var skipElseBlocks = false;
        foreach (var part in list)
        {
            if (part.IsScript)
            {
                if (part.IsIfStatement)
                {
                    // reset state
                    skipElseBlocks = false;
                    lastIfStatementCondition = false;
                }
                else if (part.IsElseIfStatement && lastIfStatementCondition)
                {
                    skipElseBlocks = true;
                    continue;
                }

                var data = jsEngine
                    .InvokeFunction(
                        part.FunctionName,
                        contextInternal);
                if (part.IsIfStatement || part.IsElseIfStatement)
                {
                    lastIfStatementCondition = !Equals(data, false);
                }
                else
                {
                    contextInternal.Page.write(data?.ToString());
                }
                continue;
            }
            if (part.IsIfBlock)
            {
                if (!lastIfStatementCondition)
                    continue;
                RenderSection(context, part.SectionName);
                continue;
            }
            if (part.IsElseIfBlock)
            {
                if (!lastIfStatementCondition || skipElseBlocks)
                    continue;
                RenderSection(context, part.SectionName);
                continue;
            }
            if (part.IsElseBlock)
            {
                if (lastIfStatementCondition)
                    continue;
                RenderSection(context, part.SectionName);
                continue;
            }
            encoding.GetBytes(
                viewContent.Slice(part.Start, part.Length),
                contextInternal.BufferWriter);
        }
        contextInternal.MarkPartRenderingDone(Path);
        contextInternal.RenderingNow = existingRenderingNow;
        contextInternal.RenderingPath = existingRenderingPath;
    }

    public void ResetCompilation()
    {
        Layout = null;

        if (Sections != null)
        {
            foreach (var section in Sections.Cast<CompiledView>())
            {
                section.ResetCompilation();
                section.Dispose();
            }
        }
        Sections = null;

        if (TextParts == null)
            return;

        var jsEngine = JavascriptEngine;
        foreach (var textPart in TextParts)
        {
            var function = textPart.FunctionName;
            if (function != null)
                jsEngine.DropFunction(function);
        }
        TextParts = null;
    }

    public void Dispose()
    {
        if (Sections != null)
        {
            foreach (var section in Sections.Cast<CompiledView>())
            {
                section.ResetCompilation();
                section.Dispose();
            }
        }
        Sections = null;

        if (ViewInternal
            .Flags
            .HasFlag(ViewFlags.PrivateJavascriptEngine))
        {
            JavascriptEngine = null;
        }
    }

    public async ValueTask<string> RenderViewToString(IViewStringRendererContext context)
    {
        await RenderView(context).ConfigureAwait(false);
        return context.RenderedString;
    }

    public string RenderViewNoLayoutToString(IViewStringRendererContext context)
    {
        RenderViewNoLayout(context);
        return context.RenderedString;
    }
}
