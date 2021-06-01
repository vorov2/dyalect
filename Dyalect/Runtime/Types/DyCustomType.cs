using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyCustomType : DyObject
    {
        internal Unit DeclaringUnit { get; }

        internal string Constructor { get; }

        internal DyTuple Privates { get; }

        internal DyCustomType(int typeCode, string ctor, DyTuple privates, Unit unit) : base(typeCode) =>
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

            var idx = Privates.GetOrdinal(name);

            if (idx == -1)
            {
                ctx.FieldNotFound();
                return;
            }

            if (Privates.IsReadOnly(idx))
            {
                ctx.FieldReadOnly();
                return;
            }

            Privates.SetValue(idx, value);
        }
    }
}
