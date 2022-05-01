using Dyalect.Debug;
using Dyalect.Runtime.Types;
namespace Dyalect.Runtime;

public sealed class DyCodeException : DyRuntimeException
{
    internal DyCodeException(DyVariant err, CallStackTrace cs, Exception? innerException)
        : base(null!, innerException) => (Error, CallTrace) = (err, cs);

    public DyCodeException(DyVariant err) : base(null!, null) => Error = err;

    public DyCodeException(DyErrorCode errorCode, params object[] args) : base(null!, null) =>
        Error = new DyVariant(errorCode, args);

    public override string Message => ErrorGenerators.GetErrorDescription(Error);

    public DyVariant Error { get; }

    public CallStackTrace? CallTrace { get; private set; }

    public override string ToString()
    {
        var errCode = ((int)ErrorGenerators.GetErrorCode(Error)).ToString().PadLeft(3, '0');
        var header = $"Error D{errCode}: {Message}";
        return CallTrace is null ? header : $"{header}\nStack trace:\n{CallTrace}";
    }
}
