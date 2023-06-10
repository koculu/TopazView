namespace Tenray.TopazView.Exceptions;

public class ScriptExecutionTimeoutException : Exception
{
    public TimeSpan MaximumScriptDuration { get; set; }

    public ScriptExecutionTimeoutException()
    {
    }

    public ScriptExecutionTimeoutException(string message) : base(message)
    {
    }

    public ScriptExecutionTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }

}