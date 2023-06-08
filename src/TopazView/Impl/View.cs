namespace Tenray.TopazView.Impl;

internal sealed class View : IView, IDisposable
{
    public string Path { get; }

    public ViewFlags Flags { get; }

    IViewEngineComponents ViewEngineComponents { get; }

    IServiceProvider ServiceProvider { get; }

    internal ICompiledView CompiledView;

    internal CompiledView UncompiledView;

    public View(string path,
        ViewFlags flags,
        IViewEngineComponents viewEngineComponents,
        IServiceProvider serviceProvider)
    {
        Path = path;
        Flags = flags;
        ViewEngineComponents = viewEngineComponents;
        ServiceProvider = serviceProvider;
        UncompiledView =
            new CompiledView(viewEngineComponents, serviceProvider, this);
    }

    public async ValueTask<ICompiledView> GetCompiledViewAsync()
    {
        var compiledView = CompiledView;
        if (compiledView != null)
            return compiledView;

        var uncompiledView = UncompiledView;
        await uncompiledView.RetrieveContent().ConfigureAwait(false);

        ViewEngineComponents
            .ViewCompiler
            .CompileView(uncompiledView);

        CompiledView = uncompiledView;
        return uncompiledView;
    }

    public ICompiledView GetCompiledView()
    {
        var compiledView = CompiledView;
        if (compiledView != null)
            return compiledView;

        var uncompiledView = UncompiledView;
        uncompiledView.RetrieveContent().Wait();

        ViewEngineComponents
            .ViewCompiler
            .CompileView(uncompiledView);

        CompiledView = uncompiledView;
        return uncompiledView;
    }

    public async ValueTask DisposeView(TimeSpan delayDispose)
    {
        CompiledView = null;
        var compiledView = UncompiledView;
        UncompiledView = new CompiledView(ViewEngineComponents, ServiceProvider, this);
        await Task.Delay(delayDispose).ConfigureAwait(false);
        compiledView.ResetCompilation();
        compiledView.Dispose();
    }

    public void Dispose()
    {
        UncompiledView.Dispose();
    }
}
