namespace Tenray.TopazView.Impl;

internal sealed class ViewEngineComponents : IViewEngineComponents
{
    public IJavascriptEngine GlobalJavascriptEngine { get; }

    public IViewRepository ViewRepository { get; }

    public IViewCompiler ViewCompiler { get; }

    public IContentProvider ContentProvider { get; }

    public ViewEngineComponents(
        IJavascriptEngine javascriptEngine,
        IViewRepository viewRepository,
        IViewCompiler viewCompiler,
        IContentProvider contentProvider)
    {
        GlobalJavascriptEngine = javascriptEngine;
        ViewRepository = viewRepository;
        ViewCompiler = viewCompiler;
        ContentProvider = contentProvider;

        ProvideViewEngineComponents(ViewRepository);
        ProvideViewEngineComponents(viewCompiler);
    }

    void ProvideViewEngineComponents(object value)
    {
        if (value is IViewEngineComponentsProvider v1)
            v1.ViewEngineComponents = this;
    }
}
