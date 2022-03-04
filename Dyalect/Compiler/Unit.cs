using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    public class Unit
    {
        internal Unit()
        {
            Layouts = new();
            Ops = new();
            Layouts = new();
            ExportList = new();
            UnitIds = new();
            References = new();
            IndexedStrings = new();
            IndexedIntegers = new();
            IndexedFloats = new();
            IndexedChars = new();
        }

        private Unit(Unit unit, DebugInfo di)
        {
            Ops = new(unit.Ops.ToArray());
            Symbols = di;

            if (unit.GlobalScope is not null)
                GlobalScope = unit.GlobalScope.Clone();

            Layouts = unit.Layouts;
            ExportList = unit.ExportList;
            UnitIds = unit.UnitIds;
            References = unit.References;
            IndexedStrings = unit.IndexedStrings;
            IndexedIntegers = unit.IndexedIntegers;
            IndexedFloats = unit.IndexedFloats;
            IndexedChars = unit.IndexedChars;
        }

        internal int Checksum { get; set; }

        internal Unit Clone(DebugInfo di) => new(this, di);

        internal int Id { get; set; }

        internal List<Reference> References { get; }

        internal List<int> UnitIds { get; }

        internal List<DyString> IndexedStrings { get; }

        internal List<DyInteger> IndexedIntegers { get; }

        internal List<DyFloat> IndexedFloats { get; }

        internal List<DyChar> IndexedChars { get; }

        internal List<Op> Ops { get; }

        public string? FileName { get; internal set; }

        public DebugInfo Symbols { get; internal set; } = null!;

        public Scope? GlobalScope { get; internal set; }

        public List<MemoryLayout> Layouts { get; }

        public Dictionary<string, ScopeVar> ExportList { get; }
    }
}