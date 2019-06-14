namespace Dyalect.Compiler
{
    internal sealed class TypeInfo
    {
        public TypeInfo(int handle, UnitInfo unit)
        {
            TypeId = handle;
            Unit = unit;
        }

        public int TypeId { get; }

        public UnitInfo Unit { get; }
    }
}
