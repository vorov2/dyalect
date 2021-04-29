using Dyalect.Compiler;
using System;
using System.Text;

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

        protected internal override bool TryGetItem(DyObject index, ExecutionContext ctx, out DyObject value)
        {
            var i = GetItemIndex(index, ctx, noerr: true);

            if (i < 0)
            {
                value = null;
                return false;
            }

            value = Locals[i].GetTaggedValue();
            return true;
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

        public override bool Equals(DyObject other)
        {
            if (TypeId == other.TypeId && other is DyCustomType t 
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

    internal sealed class DyCustomTypeInfo : DyTypeInfo
    {
        private readonly bool autoGenMethods;

        public DyCustomTypeInfo(int typeCode, string typeName, bool autoGenMethods) : base(typeCode)
        {
            TypeName = typeName;
            this.autoGenMethods = autoGenMethods;
        }

        protected override SupportedOperations GetSupportedOperations() =>
            (SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not)
            | (autoGenMethods ? (SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len)
                : SupportedOperations.None);

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DyCustomType)left;

            if (self.TypeId == right.TypeId && right is DyCustomType t && t.Constructor == self.Constructor
                && t.Locals.Length == self.Locals.Length)
            {
                for (var i = 0; i < self.Locals.Length; i++)
                {
                    var res = ctx.RuntimeContext.Types[self.Locals[i].TypeId].Eq(ctx, self.Locals[i], t.Locals[i]);

                    if (ctx.HasErrors)
                        return DyNil.Instance;

                    if (res == DyBool.False)
                        return res;
                }

                return DyBool.True;
            }

            return DyBool.False;
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var cust = (DyCustomType)arg;
            if (TypeName == cust.Constructor && cust.Locals.Length == 0)
                return new DyString($"{TypeName}()");
            else if (TypeName == cust.Constructor)
                return new DyString($"{TypeName}({LocalsToString(cust, ctx)})");
            else if (cust.Locals.Length == 0)
                return new DyString($"{TypeName}.{cust.Constructor}()");
            else
                return new DyString($"{TypeName}.{cust.Constructor}({LocalsToString(cust, ctx)})");
        }

        private string LocalsToString(DyCustomType t, ExecutionContext ctx)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < t.Locals.Length; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append(',');
                    sb.Append(' ');
                }

                sb.Append(t.Locals[i].GetLabel());
                sb.Append(':');
                sb.Append(' ');
                sb.Append(t.Locals[i].GetTaggedValue().ToString(ctx));

                if (ctx.HasErrors)
                    return "";
            }

            return sb.ToString();
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            if (autoGenMethods)
                return DyInteger.Get(((DyCustomType)arg).Locals.Length);

            return base.LengthOp(arg, ctx);
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (autoGenMethods)
                return self.GetItem(index, ctx);

            return base.GetOp(self, index, ctx);
        }

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (autoGenMethods)
            {
                self.SetItem(index, value, ctx);
                return DyNil.Instance;
            }

            return base.SetOp(self, index, value, ctx);
        }

        public override string TypeName { get; }
    }
}
