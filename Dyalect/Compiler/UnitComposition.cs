namespace Dyalect.Compiler;

public sealed class UnitComposition
{
    public FastList<Unit> Units { get; }

    public UnitComposition(FastList<Unit> units) => Units = units;
}
