using System.Runtime.Serialization;

namespace Tenray.TopazView.Exceptions;

public class ViewSyntaxException : Exception
{
    public ViewSyntaxException()
    {
    }

    public ViewSyntaxException(string message) : base(message)
    {
    }

    public ViewSyntaxException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ViewSyntaxException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
