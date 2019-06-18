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
            Types = new List<TypeDescriptor>();
            MemberIds = new List<int>();
            MemberNames = new List<string>();
            IndexedStrings = new List<DyString>();
            IndexedIntegers = new List<DyInteger>();
            IndexedFloats = new List<DyFloat>();
            IndexedChars = new List<DyChar>();
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
            Types = unit.Types;
            MemberIds = unit.MemberIds;
            MemberNames = unit.MemberNames;
            IndexedStrings = unit.IndexedStrings;
            IndexedIntegers = unit.IndexedIntegers;
            IndexedFloats = unit.IndexedFloats;
            IndexedChars = unit.IndexedChars;
        }

        internal Unit Clone(DebugInfo di) => new Unit(this, di);

        internal int Id { get; set; }

        internal List<Unit> References { get; }

        internal List<int> UnitIds { get; }

        internal List<TypeDescriptor> Types { get; }

        internal List<int> MemberIds { get; }

        internal List<string> MemberNames { get; }

        internal List<DyString> IndexedStrings { get; }

        internal List<DyInteger> IndexedIntegers { get; }

        internal List<DyFloat> IndexedFloats { get; }

        internal List<DyChar> IndexedChars { get; }

        internal List<Op> Ops { get; }

        public string FileName { get; internal set; }

        public DebugInfo Symbols { get; internal set; }

        public Scope GlobalScope { get; internal set; }

        public List<MemoryLayout> Layouts { get; }

        public List<PublishedName> ExportList { get; }
    }
}