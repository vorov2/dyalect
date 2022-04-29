namespace Dyalect;

public class DyException : Exception
{
    public DyException(string message, Exception? innerException) : base(message, innerException) { }

    public DyException(string message) : base(message, null) { }
}
