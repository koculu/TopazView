namespace Tenray.TopazView.Exceptions;

public class ViewCompilerException : Exception
{
    public ViewCompilerException()
    {
    }

    public ViewCompilerException(string message) : base(message)
    {
    }

    public ViewCompilerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}