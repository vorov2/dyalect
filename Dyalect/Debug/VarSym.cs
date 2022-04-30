namespace Dyalect.Debug;

public sealed class VarSym
{
    public string Name { get; init; } = null!;

    public int Address { get; init; }

    public int Offset { get; init; }

    public int Scope { get; init; }

    public int Flags { get; init; }

    public int Data { get; init; }

    public VarSym() { }

    public override string ToString() => Name;
}
