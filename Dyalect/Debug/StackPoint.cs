namespace Dyalect.Debug
{
    public struct StackPoint
    {
        internal StackPoint(int breakAddress, int unitHandle) => 
            (BreakAddress, UnitHandle, External) = (breakAddress, unitHandle, false);

        internal StackPoint(bool external) => 
            (BreakAddress, UnitHandle, External) = (0, 0, external);

        internal readonly bool External;

        internal readonly int BreakAddress;

        internal readonly int UnitHandle;
    }
}