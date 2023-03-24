using System.Collections.Generic;

namespace Dyalect.Compiler;

public sealed class BuilderOptions
{
    public static BuilderOptions Default() =>
        new()
        {
            Debug = false,
            NoLangModule = false,
            NoWarnings = false,
            NoWarningsLinker = false,
            LinkerSkipChecksum = false,
            KeepLazy = false
        };

    public HashSet<int> IgnoreWarnings { get; } = new();

    public bool KeepLazy { get; set; }

    public bool Debug { get; set; }

    public bool NoLangModule { get; set; }

    public bool NoWarnings { get; set; }

    public bool NoWarningsLinker { get; set; }

    public bool NoOptimizations { get; set; }

    public bool LinkerSkipChecksum { get; set; }

    public string? LinkerLog { get; set; }
}
