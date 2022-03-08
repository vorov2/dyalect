using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignObject : DyObject
    {
        protected DyForeignObject(DyTypeInfo typeInfo, Unit unit) : this(typeInfo, unit, null) { }

        protected DyForeignObject(DyTypeInfo typeInfo, Unit unit, string? ctor) : base(-1) =>
            (TypeInfo, DeclaringUnit, Constructor) = (typeInfo, unit,ctor);

        public override int TypeId => TypeInfo.ReflectedTypeCode;

        public DyTypeInfo TypeInfo { get; }

        public Unit DeclaringUnit { get; }

        public string? Constructor { get; }

        public override string GetConstructor(ExecutionContext ctx) => Constructor ?? "";

        public override int GetHashCode() => HashCode.Combine(TypeId, Constructor, DeclaringUnit.Id);
    }
}
