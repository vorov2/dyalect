using System;

namespace Dyalect.Linker
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ForeignTypeAttribute : Attribute
    {
        public ForeignTypeAttribute(string guid) => Guid = Guid.Parse(guid);

        public Guid Guid { get; }
    }
}
