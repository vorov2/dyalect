using Dyalect.Linker;
using Dyalect.Parser;
using System;

namespace Dyalect.Compiler
{
    public sealed class Reference : IEquatable<Reference>
    {
        internal static readonly Reference Empty = new(Guid.NewGuid(), "<NOTSET>", default, default, default, default);

        internal Guid Id { get; }

        internal Reference(Guid id, string moduleName, string? localPath, string? dllName, Location sourceLocation, string? sourceFleName)
        {
            Id = id;
            ModuleName = moduleName;
            LocalPath = localPath;
            DllName = dllName;
            SourceLocation = sourceLocation;
            SourceFileName = sourceFleName;
        }

        public string GetPath() =>
            LocalPath is null ? ModuleName : LocalPath + "/" + ModuleName;

        public bool Equals(Reference? other) =>
            other is not null
            && string.Equals(LocalPath, other.LocalPath, StringComparison.OrdinalIgnoreCase)
            && string.Equals(DllName, other.DllName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(ModuleName, other.ModuleName, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode() => HashCode.Combine(LocalPath, DllName, ModuleName);

        public override bool Equals(object? obj) => obj is Reference r && Equals(r);

        internal int Checksum { get; set; }

        public string? LocalPath { get; }

        public string ModuleName { get; }

        public string? DllName { get; }

        public Location SourceLocation { get; }

        public string? SourceFileName { get; }

        public ForeignUnit? Instance { get; internal set; }
    }
}
