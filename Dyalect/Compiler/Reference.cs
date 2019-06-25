using Dyalect.Parser;

namespace Dyalect.Compiler
{
    public sealed class Reference
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

        public string LocalPath { get; }

        public string ModuleName { get; }

        public string DllName { get; }

        public Location SourceLocation { get; }

        public string SourceFileName { get; }
    }
}
