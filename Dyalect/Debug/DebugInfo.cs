using System.Collections.Generic;

namespace Dyalect.Debug
{
    public sealed class DebugInfo
    {
        public DebugInfo()
        {
            Scopes = new();
            Lines = new();
            Vars = new();
            Functions = new();
        }

        private DebugInfo(DebugInfo di)
        {
            File = di.File;
            Scopes = new(di.Scopes.ToArray());
            Lines = new(di.Lines.ToArray());
            Vars = new(di.Vars.ToArray());
            Functions = new(di.Functions);
        }

        public DebugInfo Clone() => new DebugInfo(this);

        public string File { get; set; }

        public List<ScopeSym> Scopes { get; private set; }

        public List<LineSym> Lines { get; private set; }

        public List<VarSym> Vars { get; private set; }

        public Dictionary<int, FunSym> Functions { get; private set; }
    }
}
