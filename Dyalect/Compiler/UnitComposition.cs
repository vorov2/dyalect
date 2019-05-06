using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    public sealed class UnitComposition
    {
        public Guid Id = Guid.NewGuid();

        public UnitComposition(List<Unit> units)
        {
            Units = units;
            Types = StandardType.GetAll();
            Members = new FastList<string>();
            MembersMap = new Dictionary<string, int>();
            StandardTypeCount = Types.Count;
        }

        public List<Unit> Units { get; }

        public FastList<DyTypeInfo> Types { get; }

        internal int StandardTypeCount { get; }

        internal FastList<string> Members { get; }

        internal Dictionary<string, int> MembersMap { get; }
    }
}
