using Tenray.TopazView.Exceptions;

namespace Tenray.TopazView.Impl;

internal sealed class ViewCompiler : IViewCompiler, IViewEngineComponentsProvider
{
    public IViewEngineComponents ViewEngineComponents { get; set; }

    public IServiceProvider ServiceProvider { get; }

    public ViewCompiler(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    ICompiledViewInternal CompileSection(
        CompiledView parentCompiledView,
        string sectionName,
        string sectionBody)
    {
        var view = new View(
            parentCompiledView.Path + "#" + sectionName,
            parentCompiledView.ViewInternal.Flags,
            ViewEngineComponents,
            ServiceProvider);
        var compiledSection = new CompiledView(
            ViewEngineComponents, ServiceProvider, view)
        {
            ViewContent = sectionBody
        };
        CompileView(compiledSection);
        compiledSection.Layout = null;
        return compiledSection;
    }

    public void CompileView(ICompiledViewInternal uncompiledView)
    {
        try
        {
            CompileViewInternal(uncompiledView);
        }
        catch (Exception e)
        {
            throw new ViewCompilerException(
                $@"View compilation failed.
View: {uncompiledView.Path}", e);
        }
    }

    void CompileViewInternal(ICompiledViewInternal uncompiledView)
    {
        var uncompiledViewImpl = (CompiledView)uncompiledView;
        if (uncompiledViewImpl.IsCompiled)
            return;

        lock (uncompiledViewImpl.ViewCompilationLockObject)
        {
            if (uncompiledViewImpl.IsCompiled)
                return;

            var text = uncompiledViewImpl.ViewContent;
            var result = TextSplitter.SplitText(text);
            uncompiledViewImpl.Layout = result.Layout;
            uncompiledViewImpl.TextParts = result.TextParts.Where(x => !x.IsSection && !x.IsScriptSection).ToList();

            ICompiledViewInternal compileSection(TextPart textPart)
            {
                try
                {
                    return CompileSection(uncompiledViewImpl, textPart.SectionName, textPart.GetBody(text));
                }
                catch (Exception e)
                {
                    throw new SectionCompilerException(
                        $@"Section compilation failed.
View: {uncompiledView.Path}
Section: {textPart.SectionName}", e);
                }
            }

            uncompiledViewImpl.Sections = result
                .TextParts
                .Where(x => x.IsSection)
                .ToDictionary(
                    x => x.SectionName,
                    compileSection);

            ICompiledViewInternal compileScriptSection(TextPart textPart)
            {
                try
                {
                    return CompileSection(uncompiledViewImpl,
                        textPart.SectionName,
                        "@{" + textPart.GetBody(text) + "}");
                }
                catch (Exception e)
                {
                    throw new SectionCompilerException(
                        $@"Section compilation failed.
View: {uncompiledView.Path}
Section: {textPart.SectionName}", e);
                }
            }

            uncompiledViewImpl.ScriptSections = result
                .TextParts
                .Where(x => x.IsScriptSection)
                .ToDictionary(
                    x => x.SectionName,
                    compileScriptSection);

            var jsEngine = uncompiledViewImpl.JavascriptEngine;
            var list = result.TextParts;
            foreach (var textPart in list)
            {
                if (textPart.IsScript)
                {
                    var functionName = jsEngine.NextFunctionName;
                    var body = textPart.GetBody(text);
                    var script = $"function {functionName} (page, model){{{body}}}";
                    jsEngine.ExecuteScript(script);
                    textPart.FunctionName = functionName;
                }
                else if (textPart.IsIfElseBlock)
                {
                    textPart.SectionName = jsEngine.NextFunctionName +
                        (textPart.IsIfBlock ? "_if" : "_else");
                    textPart.CompiledView = compileSection(textPart);
                    uncompiledViewImpl.Sections.Add(textPart.SectionName, textPart.CompiledView);
                }
            }
            uncompiledViewImpl.IsCompiled = true;
        }
    }
}
