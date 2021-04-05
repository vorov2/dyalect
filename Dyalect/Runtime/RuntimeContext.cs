using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public sealed class RuntimeContext
    {
        internal RuntimeContext(UnitComposition composition) =>
            (Composition, Units) = (composition, new DyObject[Composition.Units.Count][]);

        internal DyObject[][] Units { get; }

        internal List<DyTypeInfo> Types => Composition.Types;

        public UnitComposition Composition { get; }
    }
}
