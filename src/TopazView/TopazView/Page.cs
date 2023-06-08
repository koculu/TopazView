﻿using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using Tenray.Topaz.API;
using Tenray.TopazView.Exceptions;
using Tenray.TopazView.Extensions;
using Tenray.TopazView.Impl;

namespace Tenray.TopazView;

public class Page : IPage
{
    IViewRenderContextInternal ViewRenderContext { get; set; }

    readonly ConcurrentQueue<ScriptDef> _scripts = new();

    readonly ConcurrentQueue<string> _styles = new();

    ScriptDef[] DistinctScripts => _scripts.Distinct().ToArray();

    string[] DistinctStyles => _styles.Distinct().ToArray();

    TextEncoder TextEncoder { get; }

    public string layout { get; set; }

    public JsObject data { get; set; } = new JsObject();

    public virtual string skin => "light";

    public Page(TextEncoder textEncoder)
    {
        TextEncoder = textEncoder;
    }

    public void renderBody()
    {
        var body = ViewRenderContext.Body;
        if (body == null)
            return;
        body.RenderViewNoLayout(ViewRenderContext);
    }

    public void renderView(object view)
    {
        if (IsValueNull(view))
            return;
        var compiledView = GetCompiledView(view.ToString());
        compiledView.RenderViewNoLayout(ViewRenderContext);
    }

    ICompiledViewInternal GetCompiledView(string viewName)
    {
        var absolutePart = ViewRenderContext.RenderingPath.JoinPath("../", viewName);

        var compiledView =
            ViewRenderContext
            .ViewEngine
            .GetOrCreateView(absolutePart, ViewRenderContext.ViewFlags)
            .GetCompiledView();

        return (ICompiledViewInternal)compiledView;
    }

    public void renderSection(object section)
    {
        renderScopeSection(section);
        if (ViewRenderContext.Body != ViewRenderContext.RenderingNow)
            renderBodySection(section);
        renderLayoutSection(section);
    }

    public void runScript(object script)
    {
        runScopeScript(script);
        if (ViewRenderContext.Body != ViewRenderContext.RenderingNow)
            runBodyScript(script);
        runLayoutScript(script);
    }

    public void renderViewSection(object view, object section)
    {
        if (IsValueNull(view) ||
            IsValueNull(section))
            return;
        var viewName = view.ToString();
        var sectionName = section.ToString();
        var compiledView = GetCompiledView(viewName);
        compiledView.RenderSection(ViewRenderContext, sectionName);
    }

    public void renderScopeSection(object section)
    {
        if (IsValueNull(section))
            return;
        var sectionName = section.ToString();

        var renderingNow = ViewRenderContext.RenderingNow;
        if (renderingNow.RenderSection(ViewRenderContext, sectionName))
            return;
    }

    public void renderBodySection(object section)
    {
        if (IsValueNull(section))
            return;
        var body = ViewRenderContext.Body;
        if (body == ViewRenderContext.RenderingNow)
            throw new RenderException("Can not render body section inside body. Use renderScopeSection instead.");
        var sectionName = section.ToString();
        if (body != null && body.RenderSection(ViewRenderContext, sectionName))
            return;
    }

    public void renderLayoutSection(object section)
    {
        if (IsValueNull(section))
            return;
        var sectionName = section.ToString();

        var layout = ViewRenderContext.Layout;
        if (layout != null && layout.RenderSection(ViewRenderContext, sectionName))
            return;
    }

    public void runViewScript(object view, object script)
    {
        if (IsValueNull(view) ||
            IsValueNull(script))
            return;
        var viewName = view.ToString();
        var scriptName = script.ToString();
        var compiledView = GetCompiledView(viewName);
        compiledView.RunScriptSection(ViewRenderContext, scriptName);
    }

    public void runScopeScript(object script)
    {
        if (IsValueNull(script))
            return;

        var scriptName = script.ToString();

        var renderingNow = ViewRenderContext.RenderingNow;
        if (renderingNow.RunScriptSection(ViewRenderContext, scriptName))
            return;
    }

    public void runBodyScript(object script)
    {
        if (IsValueNull(script))
            return;

        var body = ViewRenderContext.Body;
        if (body == ViewRenderContext.RenderingNow)
            throw new RenderException("Can not run body script inside body. Use runScopeScript instead.");

        var scriptName = script.ToString();
        if (body != null && body.RunScriptSection(ViewRenderContext, scriptName))
            return;
    }

    public void runLayoutScript(object script)
    {
        if (IsValueNull(script))
            return;

        var scriptName = script.ToString();
        var layout = ViewRenderContext.Layout;
        if (layout != null && layout.RunScriptSection(ViewRenderContext, scriptName))
            return;
    }

    public void write(object value)
    {
        if (IsValueNull(value))
            return;
        var encodedStr = TextEncoder.Encode(value.ToString());
        ViewRenderContext.Encoding.GetBytes(encodedStr, ViewRenderContext.BufferWriter);
    }

    public void raw(object value)
    {
        if (IsValueNull(value))
            return;
        var str = value.ToString();
        ViewRenderContext.Encoding.GetBytes(
            str,
            ViewRenderContext.BufferWriter);
    }

    public void addScript(object script)
    {
        if (IsValueNull(script))
            return;
        _scripts.Enqueue(new ScriptDef(script.ToString(), false));
    }

    public void addModule(object script)
    {
        if (IsValueNull(script))
            return;
        _scripts.Enqueue(new ScriptDef(script.ToString(), true));
    }

    public void addStyle(object style)
    {
        if (IsValueNull(style))
            return;
        _styles.Enqueue(style.ToString());
    }

    public void writeScripts()
    {
        foreach (var script in DistinctScripts)
        {
            if (script.IsModule)
            {
                var html = $"<script src=\"{script.Src}\" type=\"module\"></script>";
                raw(html);
            }
            else
            {
                var html = $"<script src=\"{script.Src}\"></script>";
                raw(html);
            }
        }
    }

    public void writeStyles()
    {
        foreach (var style in DistinctStyles)
        {
            var html = $"<link rel=\"stylesheet\" href=\"{style}\"/>";
            raw(html);
        }
    }

    static bool IsValueNull(object value)
    {
        return value == null;
    }

    internal void SetViewRenderContext(IViewRenderContextInternal renderContext)
    {
        ViewRenderContext = renderContext;
    }
}