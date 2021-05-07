using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyCustomType : DyObject
    {
        internal Unit DeclaringUnit { get; }

        internal string Constructor { get; }

        internal DyObject[] Locals { get; }

        internal DyCustomType(int typeCode, string ctor, DyObject[] locals, Unit unit) : base(typeCode) =>
            (Constructor, Locals, DeclaringUnit) = (ctor, locals, unit);

        public override object ToObject() => this;

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            var i = GetItemIndex(index, ctx);

            if (!ctx.HasErrors)
                ((DyLabel)Locals[i]).Value = value;
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            var i = GetItemIndex(index, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            return Locals[i].GetTaggedValue();
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) => GetOrdinal(name, ctx, noerr: true) != -1;

        private int GetItemIndex(DyObject index, ExecutionContext ctx, bool noerr = false)
        {
            if (index.TypeId == DyType.Integer)
            {
                var i = (int)index.GetInteger();
                i = i < 0 ? Locals.Length + i : i;

                if (i >= Locals.Length)
                {
                    if (!noerr)
                        ctx.IndexOutOfRange();
                    return -1;
                }

                return i;
            }

            if (index.TypeId == DyType.String)
            {
                var s = index.GetString();
                return GetOrdinal(s, ctx, noerr);
            }

            if (!noerr)
                ctx.InvalidType(index);
            return -1;
        }

        private int GetOrdinal(string s, ExecutionContext ctx, bool noerr)
        {
            for (var i = 0; i < Locals.Length; i++)
                if (Locals[i].GetLabel() == s)
                    return i;

            if (!noerr)
                ctx.IndexOutOfRange();
            return -1;
        }

        public override string GetConstructor(ExecutionContext ctx) => Constructor;

        public override int GetHashCode() => HashCode.Combine(Constructor, Locals);

        public override bool Equals(DyObject? other)
        {
            if (other is not null && TypeId == other.TypeId && other is DyCustomType t 
                && t.Constructor == Constructor && t.Locals.Length == Locals.Length)
            {
                for (var i = 0; i < Locals.Length; i++)
                    if (!t.Locals[i].Equals(Locals[i]))
                        return false;
                return true;
            }

            return false;
        }
    }
}
