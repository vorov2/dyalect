namespace Dyalect.Runtime;

internal readonly struct CatchMark
{
    public readonly int Offset;

    public readonly int StackOffset;

    public CatchMark(int offset, int stackOffset) =>
        (Offset, StackOffset) = (offset, stackOffset);
}
