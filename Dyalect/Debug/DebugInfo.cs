using System.Collections.Generic;

namespace Dyalect.Debug
{
    public sealed class DebugInfo
    {
        public DebugInfo()
        {
            Scopes = new List<ScopeSym>();
            Lines = new List<LineSym>();
            Vars = new List<VarSym>();
            Functions = new Dictionary<int, FunSym>();
        }

        private DebugInfo(DebugInfo di)
        {
            File = di.File;
            Scopes = new List<ScopeSym>(di.Scopes.ToArray());
            Lines = new List<LineSym>(di.Lines.ToArray());
            Vars = new List<VarSym>(di.Vars.ToArray());
            Functions = new Dictionary<int, FunSym>(di.Functions);
        }

        public DebugInfo Clone()
        {
            return new DebugInfo(this);
        }

        public string File { get; set; }

        public List<ScopeSym> Scopes { get; private set; }

        public List<LineSym> Lines { get; private set; }

        public List<VarSym> Vars { get; private set; }

        public Dictionary<int, FunSym> Functions { get; private set; }
    }
}
