using System.Collections.Generic;

namespace Dyalect.Debug;

public sealed class DebugInfo
{
    internal static readonly DebugInfo Default = new();

    public string? File { get; }

    public List<ScopeSym> Scopes { get; }

    public List<LineSym> Lines { get; }

    public List<VarSym> Vars { get; }

    public Dictionary<int, FunSym> Functions { get; }

    public DebugInfo() : this(default(string)) { }

    public DebugInfo(string? file)
    {
        Scopes = new();
        Lines = new();
        Vars = new();
        Functions = new();
        File = file;
    }

    private DebugInfo(DebugInfo di)
    {
        File = di.File;
        Scopes = new(di.Scopes.ToArray());
        Lines = new(di.Lines.ToArray());
        Vars = new(di.Vars.ToArray());
        Functions = new(di.Functions);
    }

    public DebugInfo Clone() => new(this);
}
