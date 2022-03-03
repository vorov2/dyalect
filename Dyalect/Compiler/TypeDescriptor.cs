using System;

namespace Dyalect.Compiler
{
    public sealed class TypeDescriptor
    {
        public TypeDescriptor(string name, int id, Type foreignTypeInfo) : this(name, id) => ForeignTypeInfo = foreignTypeInfo;

        public TypeDescriptor(string name, int id) => (Name, Id) = (name, id);

        internal bool Processed { get; set; }

        public string Name { get; }

        public int Id { get; internal set; }

        public Type? ForeignTypeInfo { get; }
    }
}
