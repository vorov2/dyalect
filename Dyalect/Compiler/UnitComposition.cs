namespace Dyalect.Compiler;

public sealed class UnitComposition
{
    public Unit[] Units { get; }

    public UnitComposition(FastList<Unit> units) => Units = units.ToArray();
}
