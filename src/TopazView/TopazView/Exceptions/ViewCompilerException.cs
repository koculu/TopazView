using System.Runtime.Serialization;

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

    protected ViewCompilerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}