namespace Dyalect.Debug;

public sealed class FunSym
{
    public string Name { get; }

    public string? TypeName { get; init; }

    public Par[]? Parameters { get; init; }

    public int StartOffset { get; init; }

    public int EndOffset { get; internal set; }

    public int Handle { get; internal set; }

    internal FunSym(string name) => Name = name;

    internal FunSym(string name, string? typeName, int offset, Par[] pars) =>
        (Name, TypeName, StartOffset, Parameters) = (name, typeName, offset, pars);
}
