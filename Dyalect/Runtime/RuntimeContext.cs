using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Linq;

namespace Dyalect.Runtime
{
    public sealed class RuntimeContext
    {
        internal RuntimeContext(UnitComposition composition)
        {
            Composition = composition;
            Types = DyType.GetAll();
            String = (DyStringTypeInfo)Types[DyType.String];
            Char = (DyCharTypeInfo)Types[DyType.Char];
            Nil = (DyNilTypeInfo)Types[DyType.Nil];
            Tuple = (DyTupleTypeInfo)Types[DyType.Tuple];
            Array = (DyArrayTypeInfo)Types[DyType.Array];
            Units = new DyObject[Composition.Units.Count][];
            Layouts = Composition.Units.Select(u => u.Layouts.ToArray()).ToArray();
        }

        public void Refresh()
        {
            var newUnits = new DyObject[Composition.Units.Count][];
            for (var i = 0; i < Units.Length; i++)
                newUnits[i] = Units[i];
            Units = newUnits;
            
            Layouts = Composition.Units.Select(u => u.Layouts.ToArray()).ToArray();
        }

        internal readonly DyStringTypeInfo String;
        internal readonly DyCharTypeInfo Char;
        internal readonly DyNilTypeInfo Nil;
        internal readonly DyTupleTypeInfo Tuple;
        internal readonly DyArrayTypeInfo Array;

        internal readonly FastList<DyTypeInfo> Types; 

        internal DyObject[][] Units { get; private set; }

        internal MemoryLayout[][] Layouts { get; private set; }

        public UnitComposition Composition { get; }
    }
}
