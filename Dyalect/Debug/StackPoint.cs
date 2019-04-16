namespace Dyalect.Debug
{
    public struct StackPoint
    {
        internal StackPoint(int breakAddress, int unitHandle)
        {
            BreakAddress = breakAddress;
            UnitHandle = unitHandle;
        }

        internal readonly int BreakAddress;

        internal readonly int UnitHandle;
    }
}