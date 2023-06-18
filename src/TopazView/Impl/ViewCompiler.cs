using Tenray.TopazView.DI;
using Tenray.TopazView.Exceptions;

namespace Tenray.TopazView.Impl;

internal sealed class ViewCompiler : IViewCompiler, IViewEngineComponentsProvider
{
    public IViewEngineComponents ViewEngineComponents { get; set; }

    public IJavascriptEngineProvider JavascriptEngineProvider { get; }

    public ViewCompiler(IJavascriptEngineProvider javascriptEngineProvider)
    {
        JavascriptEngineProvider = javascriptEngineProvider;
    }

    ICompiledViewInternal CompileSection(
        CompiledView parentCompiledView,
        string sectionName,
        string sectionBody,
        string sectionParameters)
    {
        var view = new View(
            parentCompiledView.Path + "#" + sectionName,
            parentCompiledView.ViewInternal.Flags,
            ViewEngineComponents,
            JavascriptEngineProvider);
        var compiledSection = new CompiledView(
            ViewEngineComponents, JavascriptEngineProvider, view)
        {
            ViewContent = sectionBody,
            Parameters = sectionParameters
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
                    if (textPart.IsScriptSection)
                    {
                        return CompileSection(
                            uncompiledViewImpl,
                            textPart.SectionName,
                            "@{" + textPart.GetBody(text) + "}",
                            textPart.Parameters);
                    }
                    return CompileSection(uncompiledViewImpl, textPart.SectionName, textPart.GetBody(text), textPart.Parameters);
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
                .Where(x => x.IsSectionOrScriptSection)
                .ToDictionary(
                    x => x.SectionName,
                    compileSection);

            var jsEngine = uncompiledViewImpl.JavascriptEngine;
            var list = result.TextParts;
            var parameters = uncompiledViewImpl.Parameters;
            foreach (var textPart in list)
            {
                if (textPart.IsScript)
                {
                    var functionName = jsEngine.NextFunctionName;
                    var body = textPart.GetBody(text);
                    var script = $"async function {functionName} (page,model{parameters}){{{body}}}";
                    jsEngine.ExecuteScript(script);
                    textPart.FunctionName = functionName;
                }
                else if (textPart.IsIfElseElseIfBlock)
                {
                    textPart.SectionName = jsEngine.NextFunctionName +
                        (textPart.IsIfBlock ? "_if" :
                        (textPart.IsElseIfBlock ? "_elseif" : "_else"));
                    textPart.CompiledView = compileSection(textPart);
                    uncompiledViewImpl.Sections.Add(textPart.SectionName, textPart.CompiledView);
                }
            }
            uncompiledViewImpl.IsCompiled = true;
        }
    }
}
