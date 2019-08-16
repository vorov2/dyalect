using Dyalect.Parser;
using System;

namespace Dyalect.Compiler
{
    public sealed class Reference : IEquatable<Reference>
    {
        internal Reference(string moduleName, string localPath, string dllName, Location sourceLocation, string sourceFleName)
        {
            ModuleName = moduleName;
            LocalPath = localPath;
            DllName = dllName;
            SourceLocation = sourceLocation;
            SourceFileName = sourceFleName;
        }

        public string GetPath() =>
            LocalPath == null ? ModuleName : LocalPath + "/" + ModuleName;

        public bool Equals(Reference other) =>
            !ReferenceEquals(other, null)
            && string.Equals(LocalPath, other.LocalPath, StringComparison.OrdinalIgnoreCase)
            && string.Equals(DllName, other.DllName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(ModuleName, other.ModuleName, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                if (LocalPath != null)
                    hash = hash * 23 + LocalPath.GetHashCode();

                if (DllName != null)
                    hash = hash * 23 + DllName.GetHashCode();

                if (ModuleName != null)
                    hash = hash * 23 + ModuleName.GetHashCode();

                return hash;
            }
        }

        internal int Checksum { get; set; }

        public string LocalPath { get; }

        public string ModuleName { get; }

        public string DllName { get; }

        public Location SourceLocation { get; }

        public string SourceFileName { get; }
    }
}
