using System.Runtime.Serialization;

namespace Tenray.TopazView.Exceptions;

public class ViewFactoryException : Exception
{
    public ViewFactoryException()
    {
    }

    public ViewFactoryException(string message) : base(message)
    {
    }

    public ViewFactoryException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ViewFactoryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}