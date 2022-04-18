namespace Dyalect.Compiler
{
    public sealed class UnitComposition
    {
        public Unit[] Units { get; }

        public UnitComposition(Unit[] units) => Units = units;
    }
}
