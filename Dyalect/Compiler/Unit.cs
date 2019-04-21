using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    public class Unit
    {
        internal Unit()
        {
            Layouts = new List<MemoryLayout>();
            Ops = new List<Op>();
            Layouts = new List<MemoryLayout>();
            ExportList = new List<PublishedName>();
            UnitIds = new List<int>();
            References = new List<Unit>();
            TypeIds = new List<int>();
            TypeNames = new List<string>();
            IndexedStrings = new List<DyString>();
            IndexedIntegers = new List<DyInteger>();
            IndexedFloats = new List<DyFloat>();
        }

        private Unit(Unit unit, DebugInfo di)
        {
            Ops = new List<Op>(unit.Ops.ToArray());
            Symbols = di;

            if (unit.GlobalScope != null)
                GlobalScope = unit.GlobalScope.Clone();

            Layouts = unit.Layouts;
            ExportList = unit.ExportList;
            UnitIds = unit.UnitIds;
            References = unit.References;
            TypeIds = unit.TypeIds;
            TypeNames = unit.TypeNames;
            IndexedStrings = unit.IndexedStrings;
            IndexedIntegers = unit.IndexedIntegers;
            IndexedFloats = unit.IndexedFloats;
        }

        internal Unit Clone(DebugInfo di) => new Unit(this, di);

        internal int Id { get; set; }

        internal List<Unit> References { get; }

        internal List<int> UnitIds { get; }

        internal List<int> TypeIds { get; }

        internal List<string> TypeNames { get; }

        internal List<DyString> IndexedStrings { get; }

        internal List<DyInteger> IndexedIntegers { get; }

        internal List<DyFloat> IndexedFloats { get; }

        internal List<Op> Ops { get; }

        public string FileName { get; set; }

        public DebugInfo Symbols { get; set; }

        public Scope GlobalScope { get; set; }

        public List<MemoryLayout> Layouts { get; }

        public List<PublishedName> ExportList { get; }
    }
}