namespace Dyalect
{
    public class BuildMessage
    {
        private const string ERR_FORMAT = "{0}({1},{2}): {3} Dy{4}: {5}";

        public BuildMessage(string message, BuildMessageType type, int code, int line, int col, string file)
        {
            Message = message;
            Type = type;
            Code = code;
            Line = line;
            Column = col;
            File = file;
        }

        public override string ToString()
        {
            var stype = Type == BuildMessageType.Error ? "Ошибка"
                : Type == BuildMessageType.Warning ? "Предупреждение"
                : Type == BuildMessageType.Hint ? "Подсказка"
                : "";
            var scode = Code.ToString().PadLeft(3, '0');
            return string.Format(ERR_FORMAT, GetFileName(), Line, Column, stype, scode, Message);
        }

        protected string GetFileName()
        {
            return File == null ? "<memory>" : File;
        }

        public string File { get; internal set; }

        public string Message { get; protected set; }

        public BuildMessageType Type { get; }

        public int Code { get; protected set; }

        public int Line { get; protected set; }

        public int Column { get; protected set; }
    }
}
