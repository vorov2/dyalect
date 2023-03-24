using System.Collections.Generic;

namespace Dyalect.Debug;

public sealed class DebugInfo
{
    internal static readonly DebugInfo Default = new();

    public DebugInfo()
    {
        Scopes = new();
        Lines = new();
        Vars = new();
        Functions = new();
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

    public string? File { get; set; }

    public List<ScopeSym> Scopes { get; }

    public List<LineSym> Lines { get; }

    public List<VarSym> Vars { get; }

    public Dictionary<int, FunSym> Functions { get; }
}
