using System.Collections.Generic;
using Dyalect.Library;

internal static class LibraryMeta
{
    public const string Version = "0.1.0";

    public const string Product = "Dyalect Standard Library";

    public const string Company = "Vasily Voronkov";

    public const string Description = "Dyalect Standard Library";

    public const string Copyright = "Copyright (c) Vasily Voronkov 2020";

    public const string Trademark = "Vasily Voronkov";

    public static readonly IReadOnlyDictionary<string, System.Type> Modules = new Dictionary<string, System.Type>
    {
         { "core", typeof(Core) }
        ,{ "math", typeof(Math) }
    };
}