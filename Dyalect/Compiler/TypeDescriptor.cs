using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Compiler
{
    public sealed class TypeDescriptor
    {
        public TypeDescriptor(string name, int id, bool autoGenCons, Func<int, DyTypeInfo> act) :
            this(name, id, autoGenCons)
        {
            TypeInfoActivator = act;
        }

        public TypeDescriptor(string name, int id, bool autoGenCons)
        {
            Name = name;
            Id = id;
            AutoGenConstructors = autoGenCons;
        }

        public string Name { get; }
        public int Id { get; internal set; }
        public bool AutoGenConstructors { get; }
        internal bool Processed { get; set; }
        public Func<int, DyTypeInfo> TypeInfoActivator { get; }
    }
}
