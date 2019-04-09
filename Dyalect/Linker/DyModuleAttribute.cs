using System;

namespace Dyalect.Linker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DyModuleAttribute : Attribute
    {
        public DyModuleAttribute(string moduleName)
        {
            ModuleName = moduleName;
        }

        public string ModuleName { get; }
    }
}
