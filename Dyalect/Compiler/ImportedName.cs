namespace Dyalect.Compiler
{
    internal sealed class ImportedName
    {
        public ImportedName(string moduleName, int moduleHandle, PublishedName name)
        {
            ModuleName = moduleName;
            ModuleHandle = moduleHandle;
            PublishedName = name;
        }

        public string ModuleName { get; }

        public int ModuleHandle { get; }

        public PublishedName PublishedName { get; }
    }
}
