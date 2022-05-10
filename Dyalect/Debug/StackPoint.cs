namespace Dyalect.Debug;

public readonly struct StackPoint
{
    public static readonly StackPoint External = new(external: true);

    internal static readonly StackPoint Empty = new(-1, -1);

    public readonly int Offset;

    public readonly int UnitId;

    public readonly bool IsExternal;

    public bool IsEmpty => Offset == -1;

    internal StackPoint(int offset, int unitId) => 
        (Offset, UnitId, IsExternal) = (offset, unitId, false);

    private StackPoint(bool external) =>
        (Offset, UnitId, IsExternal) = (0, 0, external);
}
