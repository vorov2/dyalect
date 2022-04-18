using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public sealed class RuntimeContext
    {
        internal RuntimeContext(UnitComposition composition)
        {
            (Composition, Units) = (composition, new DyObject[composition.Units.Length][]);
            Types = DyType.GetAll();
            String = (DyStringTypeInfo)Types[DyType.String];
            Char = (DyCharTypeInfo)Types[DyType.Char];
            Nil = (DyNilTypeInfo)Types[DyType.Nil];
            Tuple = (DyTupleTypeInfo)Types[DyType.Tuple];
            Array = (DyArrayTypeInfo)Types[DyType.Array];
        }

        internal readonly DyStringTypeInfo String;
        internal readonly DyCharTypeInfo Char;
        internal readonly DyNilTypeInfo Nil;
        internal readonly DyTupleTypeInfo Tuple;
        internal readonly DyArrayTypeInfo Array;

        internal readonly List<DyTypeInfo> Types; 

        internal DyObject[][] Units { get; }

        public UnitComposition Composition { get; }
    }
}
