namespace Dyalect.Debug;

public sealed class LineSym
{
    internal static readonly LineSym Empty = new(0);

    public int Offset { get; }

    public int Line { get; }

    public int Column { get; }

    internal LineSym(int offset) => Offset = offset;

    internal LineSym(int offset, int line, int column) => 
        (Offset, Line, Column) = (offset, line, column);
}
