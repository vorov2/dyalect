namespace Dyalect.Compiler
{
    internal sealed class TypeInfo
    {
        public TypeInfo(int handle, UnitInfo unit) => (TypeId, Unit) = (handle, unit);

        public int TypeId { get; }

        public UnitInfo Unit { get; }
    }
}
