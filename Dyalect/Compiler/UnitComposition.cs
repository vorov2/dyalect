using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    public sealed class UnitComposition
    {
        internal UnitComposition(List<Unit> units)
        {
            Units = units;
            Types = StandardType.All.Clone();
        }

        public List<Unit> Units { get; }

        public FastList<DyType> Types { get; }
    }    
}
