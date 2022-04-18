using System.Collections.Generic;

namespace Dyalect.Compiler
{
    internal sealed class UnitInfo
    {
        public UnitInfo(int handle, Dictionary<HashString, ScopeVar> exportList) => (Handle, ExportList) = (handle, exportList);

        public int Handle { get; }

        public Dictionary<HashString, ScopeVar> ExportList { get; }
    }
}
