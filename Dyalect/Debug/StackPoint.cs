namespace Dyalect.Debug;

public readonly struct StackPoint
{
    public static readonly StackPoint External = new(external: true);

    private readonly bool external;

    public readonly int BreakAddress;

    public readonly int UnitHandle;

    internal StackPoint(int breakAddress, int unitHandle) => 
        (BreakAddress, UnitHandle, external) = (breakAddress, unitHandle, false);

    private StackPoint(bool external) =>
        (BreakAddress, UnitHandle, this.external) = (0, 0, external);

    public bool IsExternal => external;
}
