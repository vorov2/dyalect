namespace Dyalect.Debug
{
    public class CallFrame
    {
        private const string FORMAT_LNG = "\tat {0} in {1}, line {2}, column {3}";
        private const string FORMAT_SHT = "\tat {0} in {1}, offset {2}";
        internal static readonly CallFrame External = new ExternalCallFrame();

        sealed class ExternalCallFrame : CallFrame
        {
            public override string ToString() => "\tat <external code>";
        }

        private CallFrame() { }

        internal CallFrame(string moduleName, string codeBlockName, int offset, LineSym lineSym)
        {
            CodeBlockName = codeBlockName;
            ModuleName = moduleName;
            Offset = offset;
            LinePragma = lineSym;
        }
        
        public override string ToString() =>
            LinePragma != null 
                ? string.Format(FORMAT_LNG, GetName(), ModuleName, LinePragma.Line, LinePragma.Column)
                : string.Format(FORMAT_SHT, GetName(), ModuleName, Offset);
        
        private string GetName() => CodeBlockName ?? "<global>";

        public string CodeBlockName { get; }

        public string ModuleName { get; }

        public int Offset { get; }

        public LineSym LinePragma { get; }
    }
}