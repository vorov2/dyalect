using Dyalect.Parser;

namespace Dyalect.Compiler
{
    public sealed class Reference
    {
        internal Reference(string moduleName, string dllName, Location sourceLocation, string sourceFleName)
        {
            ModuleName = moduleName;
            DllName = dllName;
            SourceLocation = sourceLocation;
            SourceFileName = sourceFleName;
        }

        public string ModuleName { get; }

        public string DllName { get; }

        public Location SourceLocation { get; }

        public string SourceFileName { get; }
    }
}
