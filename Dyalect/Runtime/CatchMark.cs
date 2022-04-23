namespace Dyalect.Runtime;

internal readonly struct CatchMark
{
    public CatchMark(int offset, int stackOffset) =>
        (Offset, StackOffset) = (offset, stackOffset);

    public readonly int Offset;

    public readonly int StackOffset;
}
