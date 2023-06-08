using System.Runtime.Serialization;

namespace Tenray.TopazView.Exceptions;

public class SectionCompilerException : Exception
{
    public SectionCompilerException()
    {
    }

    public SectionCompilerException(string message) : base(message)
    {
    }

    public SectionCompilerException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected SectionCompilerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}