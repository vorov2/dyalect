namespace Dyalect.Debug;

public sealed class FunSym
{
    public string Name { get; }

    public Par[]? Parameters { get; init; }

    public int StartOffset { get; init; }

    public int EndOffset { get; internal set; }

    public int Handle { get; internal set; }

    internal FunSym(string name) => Name = name;

    internal FunSym(string name, int offset, Par[] pars) => (Name, StartOffset, Parameters) = (name, offset, pars);
}
