namespace Tenray.TopazView;

public interface IView
{
    string Path { get; }

    public ViewFlags Flags { get; }

    ValueTask<ICompiledView> GetCompiledViewAsync();

    ICompiledView GetCompiledView();

    /// <summary>
    /// Dispose the view resources.
    /// </summary>
    /// <param name="delayDispose">Delay the dispose operation 
    /// in order to give room for active render calls finalize.</param>
    /// <returns></returns>
    ValueTask DisposeView(TimeSpan delayDispose);
}
