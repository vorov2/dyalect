using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Linq;
namespace Dyalect.Runtime;

public sealed class RuntimeContext
{
    internal RuntimeContext(UnitComposition composition)
    {
        Types = Dy.GetAll();
        String = (DyStringTypeInfo)Types[Dy.String];
        Char = (DyCharTypeInfo)Types[Dy.Char];
        Nil = (DyNilTypeInfo)Types[Dy.Nil];
        Tuple = (DyTupleTypeInfo)Types[Dy.Tuple];
        Array = (DyArrayTypeInfo)Types[Dy.Array];
        Composition = composition;
        Units = new DyObject[Composition.Units.Length][];
        Layouts = Composition.Units.Select(u => u.Layouts.UnsafeGetArray()).ToArray();
    }

    public void Refresh(UnitComposition composition)
    {
        Composition = composition;

        //Take into account new modules
        var newUnits = new DyObject[Composition.Units.Length][];
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

    public UnitComposition Composition { get; private set; }
}
