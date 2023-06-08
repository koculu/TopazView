namespace Tenray.TopazView;

public interface ICompiledView
{
    string Path { get; }

    ValueTask RenderView(IViewRenderContext context);

    ValueTask<string> RenderViewToString(IViewStringRendererContext context);

    void RenderViewNoLayout(IViewRenderContext context);

    string RenderViewNoLayoutToString(IViewStringRendererContext context);
}