using System;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    public sealed class UnitComposition
    {
        public Guid Id { get; } = Guid.NewGuid();

        public List<Unit> Units { get; }

        public UnitComposition(List<Unit> units) => Units = units;
    }
}
