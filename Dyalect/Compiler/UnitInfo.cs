namespace Dyalect.Compiler
{
    internal sealed class UnitInfo
    {
        public UnitInfo(int handle, Unit unit) => (Handle, Unit) = (handle, unit);

        public int Handle { get; }

        public Unit Unit { get; }
    }
}
