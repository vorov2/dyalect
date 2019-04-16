namespace Dyalect.Debug
{
    public struct StackPoint
    {
        internal StackPoint(int breakAddress, int moduleHandle)
        {
            BreakAddress = breakAddress;
            ModuleHandle = moduleHandle;
        }

        internal readonly int BreakAddress;

        internal readonly int ModuleHandle;
    }
}