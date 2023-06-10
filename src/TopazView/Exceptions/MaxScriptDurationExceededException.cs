namespace Tenray.TopazView.Exceptions;

public class MaxScriptDurationExceededException : Exception
{
    public MaxScriptDurationExceededException()
    {
    }

    public MaxScriptDurationExceededException(string message) : base(message)
    {
    }

    public MaxScriptDurationExceededException(string message, Exception innerException) : base(message, innerException)
    {
    }

}