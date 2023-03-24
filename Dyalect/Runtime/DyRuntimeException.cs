namespace Dyalect.Runtime;

public class DyRuntimeException : DyException
{
    public DyRuntimeException(string message, Exception? innerException) : base(message, innerException) { }

    public DyRuntimeException(string message) : base(message) { }
}
