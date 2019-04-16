using System;
using Dyalect.Debug;

namespace Dyalect.Runtime
{
    public sealed class DyCodeException : DyRuntimeException
    {
        public DyCodeException(DyError err, CallStackTrace cs, Exception innerException) : base(null, innerException)
        {
            Error = err;
            CallStack = cs;
        }

        public override string Message => Error.GetDescription();

        public DyError Error { get; private set; }

        public CallStackTrace CallStack { get; private set; }

        public override string ToString()
        {
            var errCode = ((int)Error.Code).ToString().PadLeft(3, '0');
            return $"Runtime exception Dy{errCode}: {Message}\nStack trace:\n{CallStack}";
        }
    }
}
