namespace Dyalect.Debug
{
    public sealed class CallFrame
    {
        private const string FORMAT_LNG = "\tat {0} in {1}, line {2}, column {3}";
        private const string FORMAT_SHT = "\tat {0} in {1}, offset {2}";

        internal CallFrame(bool global, string moduleName, string name, int offset, LineSym lineSym)
        {
            Global = global;
            Name = name;
            ModuleName = moduleName;
            Offset = offset;
            LinePragma = lineSym;
        }
        
        public override string ToString()
        {
            return LinePragma != null ?
                string.Format(FORMAT_LNG, GetFullName(), ModuleName, LinePragma.Line, LinePragma.Column) :
                string.Format(FORMAT_SHT, GetFullName(), ModuleName, Offset);
        }
        
        private string GetFullName()
        {
            return Name != null ? Name : "<global>";
        }
        
        public bool Global { get; private set; }

        public string Name { get; private set; }

        public string ModuleName { get; private set; }

        public int Offset { get; private set; }

        public LineSym LinePragma { get; private set; }
    }
}