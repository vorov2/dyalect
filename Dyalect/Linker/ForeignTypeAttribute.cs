using System;

namespace Dyalect.Linker
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ForeignTypeAttribute : Attribute
    {
        public ForeignTypeAttribute(Guid guid) => Guid = guid;

        public Guid Guid { get; }
    }
}
