using System;

namespace Dyalect.Linker
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ForeignTypeAttribute : Attribute
    {
        public ForeignTypeAttribute(string guid, params string[] constructors) =>
            (Guid, Constructors) = (Guid.Parse(guid), constructors);

        public Guid Guid { get; }

        public string[] Constructors { get; }
    }
}
