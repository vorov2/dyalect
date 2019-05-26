using System;
using Dyalect.Debug;

namespace Dyalect.Runtime
{
    public sealed class DyCodeException : DyRuntimeException
    {
        internal DyCodeException(DyError err, CallStack callStack, CallStackTrace cs, Exception innerException) : base(null, innerException)
        {
            Error = err;
            CallTrace = cs;
            CallStack = callStack;
        }

        internal CallStack CallStack { get; }

        public override string Message => Error.GetDescription();

        public DyError Error { get; private set; }

        public CallStackTrace CallTrace { get; private set; }

        public override string ToString()
        {
            var errCode = ((int)Error.Code).ToString().PadLeft(3, '0');
            return $"Runtime exception Dy{errCode}: {Message}\nStack trace:\n{CallStack}";
        }
    }
}
