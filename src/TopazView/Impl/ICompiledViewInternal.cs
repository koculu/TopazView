namespace Tenray.TopazView.Impl;

internal interface ICompiledViewInternal : ICompiledView
{
    bool RenderSection(IViewRenderContext context, string sectionName);

    bool RunScriptSection(IViewRenderContext context, string scriptName);
}
