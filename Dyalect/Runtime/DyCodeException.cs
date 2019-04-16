using System;
using Dyalect.Debug;

namespace Dyalect.Runtime
{
    public sealed class DyCodeException : DyRuntimeException
    {
        public DyCodeException(DyError err, string file, int line, int column, CallStackTrace cs,
            Exception innerException) : base(null, innerException)
        {
            Error = err;
            File = file;
            Line = line;
            Column = column;
            CallStack = cs;
        }

        public override string Message => Error.GetDescription();

        public DyError Error { get; private set; }

        public string File { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public CallStackTrace CallStack { get; private set; }

        public override string ToString()
        {
            var errCode = ((int)Error.Code).ToString().PadLeft(3, '0');
            var baseStr = $"Runtime exception Dy{errCode}: {Message}\nLocation: {File}, line {Line}, column {Column}.";

            if (CallStack.FrameCount > 0)
                baseStr += $"\nStack trace:\n{CallStack}";

            return baseStr;
        }
    }
}
