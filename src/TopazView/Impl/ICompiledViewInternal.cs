namespace Tenray.TopazView.Impl;

internal interface ICompiledViewInternal : ICompiledView
{
    bool RenderSection(IViewRenderContext context, string sectionName, params object[] args);
}
