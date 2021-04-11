using System;

namespace Dyalect.Linker
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class FunctionAttribute : Attribute
    {
        public FunctionAttribute(string name) => Name = name;

        public string Name { get; }

        public override string ToString() => Name;
    }
}
