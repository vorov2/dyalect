namespace Dyalect.Parser.Model;

public sealed class DImport
{
    public DImport(Location loc) => Location = loc;

    public string? Alias { get; set; }

    public string ModuleName { get; set; } = null!;

    public string? LocalPath { get; set; }

    public Location Location { get; }

    public override string ToString()
    {
        var alias = Alias is null ? "" : $" {Alias} = ";
        var path = LocalPath is null ? "" : $"{LocalPath}/";
        return $"import {Alias}{LocalPath}{ModuleName}";
    }
}
