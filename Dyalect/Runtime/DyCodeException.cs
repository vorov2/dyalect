using System;
using Dyalect.Debug;
using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public sealed class DyCodeException : DyRuntimeException
    {
        internal DyCodeException(DyError err, CallStackTrace cs, Exception? innerException)
            : base(null!, innerException) => (Error, CallTrace) = (err, cs);

        public override string Message => Error.GetDescription();

        public DyError Error { get; private set; }

        public CallStackTrace CallTrace { get; private set; }

        public override string ToString()
        {
            var errCode = ((int)Error.Code).ToString().PadLeft(3, '0');
            return $"Error D{errCode}: {Message}\nStack trace:\n{CallTrace}";
        }
    }
}
