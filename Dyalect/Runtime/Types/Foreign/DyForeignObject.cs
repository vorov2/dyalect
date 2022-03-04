using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignObject<T> : DyObject where T : ForeignTypeInfo
    {
        protected DyForeignObject(DyTypeInfo typeInfo, Unit unit) : this(typeInfo, unit, null) { }

        protected DyForeignObject(DyTypeInfo typeInfo, Unit unit, string? ctor) : base(typeInfo) =>
            (DeclaringUnit, Constructor) = (unit,ctor);

        public Unit DeclaringUnit { get; }

        public string? Constructor { get; }

        public override void GetConstructor(ExecutionContext ctx, out string ctor, out bool priv) => (ctor, priv) = (Constructor ?? "", false);

        public override int GetHashCode() => HashCode.Combine((int)DecType.TypeCode, Constructor, DeclaringUnit.Id);
    }
}
