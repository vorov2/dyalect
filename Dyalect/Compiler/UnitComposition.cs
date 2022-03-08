using System.Collections.Generic;

namespace Dyalect.Compiler
{
    public sealed class UnitComposition
    {
        public List<Unit> Units { get; }

        public UnitComposition(List<Unit> units) => Units = units;
    }
}
