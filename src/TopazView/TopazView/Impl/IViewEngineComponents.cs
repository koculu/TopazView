namespace Tenray.TopazView.Impl;

internal interface IViewEngineComponents
{
    IJavascriptEngine GlobalJavascriptEngine { get; }

    IViewRepository ViewRepository { get; }

    IViewCompiler ViewCompiler { get; }

    IContentProvider ContentProvider { get; }
}
