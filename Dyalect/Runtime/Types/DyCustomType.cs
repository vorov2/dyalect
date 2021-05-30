using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyCustomType : DyObject
    {
        internal Unit DeclaringUnit { get; }

        internal string Constructor { get; }

        internal DyObject Privates { get; }

        internal DyCustomType(int typeCode, string ctor, DyObject privates, Unit unit) : base(typeCode) =>
            (Constructor, Privates, DeclaringUnit) = (ctor, privates, unit);

        public override object ToObject() => this;

        public override string GetConstructor(ExecutionContext ctx) => Constructor;

        public override int GetHashCode() => HashCode.Combine(Constructor, Privates);

        public override bool Equals(DyObject? other) =>
            other is not null && TypeId == other.TypeId && other is DyCustomType t 
                && t.Constructor == Constructor && t.Privates.Equals(Privates);

        internal override void SetPrivate(ExecutionContext ctx, string name, DyObject value)
        {
            if (ctx.TypeStack.Count == 0 || TypeId != ctx.TypeStack.Peek())
            {
                ctx.PrivateAccess();
                return;
            }

            if (Privates is not DyAssemblage asm)
            {
                ctx.FieldNotFound();
                return;
            }

            var idx = asm.GetOrdinal(name);

            if (idx == -1)
            {
                ctx.FieldNotFound();
                return;
            }

            if (asm.IsReadOnly(idx))
            {
                ctx.FieldReadOnly();
                return;
            }

            asm.Values[idx] = value;
        }
    }
}
