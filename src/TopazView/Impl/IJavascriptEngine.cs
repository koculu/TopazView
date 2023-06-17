namespace Tenray.TopazView.Impl;

internal interface IJavascriptEngine
{
    string NextFunctionName { get; }

    void ExecuteScript(string script);

    object InvokeFunction(
        string functionName,
        IViewRenderContext renderContext,
        params object[] args);

    void DropFunction(string function);
}
