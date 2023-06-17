namespace Tenray.TopazView;

public interface ICompiledView
{
    string Path { get; }

    ValueTask RenderView(IViewRenderContext context, params object[] args);

    ValueTask<string> RenderViewToString(IViewStringRendererContext context, params object[] args);

    void RenderViewNoLayout(IViewRenderContext context, params object[] args);

    string RenderViewNoLayoutToString(IViewStringRendererContext context, params object[] args);
}