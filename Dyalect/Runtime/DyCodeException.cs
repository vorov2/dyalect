using System;
using Dyalect.Debug;
using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public sealed class DyCodeException : DyRuntimeException
    {
        internal DyCodeException(DyVariant err, CallStackTrace cs, Exception? innerException)
            : base(null!, innerException) => (Error, CallTrace) = (err, cs);

        public override string Message => ErrorGenerators.GetErrorDescription(Error);

        public DyVariant Error { get; private set; }

        public CallStackTrace CallTrace { get; private set; }

        public override string ToString()
        {
            var errCode = ((int)ErrorGenerators.GetErrorCode(Error)).ToString().PadLeft(3, '0');
            return $"Error D{errCode}: {Message}\nStack trace:\n{CallTrace}";
        }
    }
}
