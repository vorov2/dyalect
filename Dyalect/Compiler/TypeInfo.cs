namespace Dyalect.Compiler
{
    internal sealed class TypeInfo
    {
        public TypeInfo(int handle, UnitInfo unit)
        {
            Handle = handle;
            Unit = unit;
        }

        public int Handle { get; }

        public UnitInfo Unit { get; }
    }
}
