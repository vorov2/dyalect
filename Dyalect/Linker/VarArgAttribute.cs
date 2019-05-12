using System;

namespace Dyalect.Linker
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class VarArgAttribute : Attribute
    {
    }
}
