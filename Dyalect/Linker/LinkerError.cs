namespace Dyalect.Linker
{
    public enum LinkerError
    {
        None,

        ModuleNotFound = 200,

        UnableReadModule = 201,

        UnableLoadAssembly = 202,

        DuplicateModuleName = 203,

        AssemblyModuleNotFound = 204,

        AssemblyModuleLoadError = 205,

        InvalidAssemblyModule = 206
    }
}
