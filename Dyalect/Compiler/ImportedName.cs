namespace Dyalect.Compiler
{
    internal sealed class ImportedName
    {
        public ImportedName(string moduleName, int moduleHandle, string name, ScopeVar var)
        {
            ModuleName = moduleName;
            ModuleHandle = moduleHandle;
            PublishedName = name;
            Var = var;
        }

        public string ModuleName { get; }

        public int ModuleHandle { get; }

        public string PublishedName { get; }

        public ScopeVar Var { get; }
    }
}
