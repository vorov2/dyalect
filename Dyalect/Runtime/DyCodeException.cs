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
            return string.Format("Ошибка времени исполнения Dy{1}: {2}{0}Место: {3}, строка: {4}, столбец: {5}{0}Трассировка:{0}{6}",
                Environment.NewLine, (int)Error.Code, Message, File, Line, Column, CallStack);
        }
    }
}
