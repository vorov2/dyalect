using System;

namespace Dyalect.Compiler
{
    public sealed class TypeDescriptor
    {
        public TypeDescriptor(string name, int id, bool autoGenCons, Type foreignTypeInfo) :
            this(name, id, autoGenCons) => ForeignTypeInfo = foreignTypeInfo;

        public TypeDescriptor(string name, int id, bool autoGenCons) =>
            (Name, Id, AutoGenConstructors) = (name, id, autoGenCons);

        internal bool Processed { get; set; }
        
        public string Name { get; }
        
        public int Id { get; internal set; }
        
        public bool AutoGenConstructors { get; }

        public Type? ForeignTypeInfo { get; }
    }
}
