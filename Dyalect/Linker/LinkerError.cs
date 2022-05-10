namespace Dyalect.Linker;

public enum LinkerError
{
    None,

    ModuleNotFound = 400,

    UnableReadModule = 401,

    UnableLoadAssembly = 402,

    DuplicateModuleName = 403,

    AssemblyModuleNotFound = 404,

    AssemblyModuleLoadError = 405,

    InvalidAssemblyModule = 406,

    UnableReadObjectFile = 407,

    ChecksumValidationFailed = 408,

    AssemblyNotFound = 409,

    InvalidForeignModule = 410
}
