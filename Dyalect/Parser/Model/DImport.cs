﻿namespace Dyalect.Parser.Model
{
    public sealed class DImport
    {
        public DImport(Location loc) => Location = loc;

        public string? Alias { get; set; }

        public string ModuleName { get; set; } = null!;

        public string? LocalPath { get; set; }

        public Location Location { get; }
    }
}
