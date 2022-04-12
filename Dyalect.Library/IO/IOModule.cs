using Dyalect.Library.Core;
using Dyalect.Linker;

namespace Dyalect.Library.IO
{
    [DyUnit("io")]
    public sealed class IOModule : ForeignUnit
    {
        public readonly Reference<CoreModule> Core;

        public DyFileTypeInfo File { get; }
        public DyPathTypeInfo Path { get; }
        public DyDirectoryTypeInfo Directory { get; }
        public DyDriveTypeInfo Drive { get; }

        public IOModule()
        {
            Core = AddReference<CoreModule>();
            File = AddType<DyFileTypeInfo>();
            Path = AddType<DyPathTypeInfo>();
            Directory = AddType<DyDirectoryTypeInfo>();
            Drive = AddType<DyDriveTypeInfo>();
        }
    }
}
