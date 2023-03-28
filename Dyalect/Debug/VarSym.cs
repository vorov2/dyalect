namespace Dyalect.Debug;

public sealed class VarSym
{
    public required string Name { get; init; }

    public int Address { get; init; }

    public int Offset { get; init; }

    public int Scope { get; init; }

    public int Flags { get; init; }

    public int Data { get; init; }

    public override string ToString() => Name;
}
