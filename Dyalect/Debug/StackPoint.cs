namespace Dyalect.Debug
{
    public struct StackPoint
    {
        internal StackPoint(int breakAddress, int unitHandle)
        {
            BreakAddress = breakAddress;
            UnitHandle = unitHandle;
            External = false;
        }

        internal StackPoint(bool external)
        {
            BreakAddress = 0;
            UnitHandle = 0;
            External = external;
        }

        internal readonly bool External;

        internal readonly int BreakAddress;

        internal readonly int UnitHandle;
    }
}