namespace Dyalect.Compiler;

public readonly struct ScopeVar
{
    public static readonly ScopeVar Empty = new(-1, 0);

    public readonly int Address;

    public readonly int Data;

    public readonly int Args;

    public ScopeVar(int address) : this(address, 0, 0) { }

    public ScopeVar(int address, int data) : this(address, data, 0) { }

    public ScopeVar(int address, int data, int args) => (Address, Data, Args) = (address, data, args);

    public bool IsEmpty() => Address == -1;
}
