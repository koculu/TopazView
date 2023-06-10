using Tenray.Topaz;
using Tenray.TopazView.Exceptions;

namespace Tenray.TopazView.Impl;

internal sealed class JavascriptEngine : IJavascriptEngine
{
    int NextFunctionId;

    TopazEngine Engine { get; }

    public string NextFunctionName
    {
        get
        {
            var id = Interlocked.Increment(ref NextFunctionId);
            return "_f" + id;
        }
    }

    public JavascriptEngine(ITopazFactory topazFactory)
    {
        Engine = topazFactory.CreateTopazEngine();
    }

    public void AddGlobalObject(string name, object value)
    {
        Engine.SetValue(name, value);
    }

    public void DropFunction(string function)
    {
        if (string.IsNullOrWhiteSpace(function))
            return;
        var script = function + " = undefined";
        Engine.ExecuteScript(script);
    }

    public void ExecuteScript(string script)
    {
        try
        {
            Engine.ExecuteScript(script);
        }
        catch (Exception ex)
        {
            throw new AggregateException($"Script failed:{Environment.NewLine}{script}", ex);
        }
    }

    public object InvokeFunction(
        string functionName,
        IViewRenderContext renderContext)
    {
        using var cancellationSource = new CancellationTokenSource(renderContext.MaximumScriptDuration);
        try
        {
            var value = Engine
                .InvokeFunction(
                    functionName,
                    cancellationSource.Token,
                    renderContext.Page,
                    renderContext.Model);
            return value;
        }
        catch (OperationCanceledException e)
        {
            throw new ScriptExecutionTimeoutException
                (@$"The script execution time has exceeded the maximum duration allowed in the render context.
    MaximumScriptDuration: {renderContext.MaximumScriptDuration.Seconds} seconds", e)
            {
                MaximumScriptDuration = renderContext.MaximumScriptDuration
            };
        }

    }
}
