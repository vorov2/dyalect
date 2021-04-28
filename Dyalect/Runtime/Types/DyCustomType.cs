using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyCustomType : DyObject
    {
        internal Unit DeclaringUnit { get; }

        internal string Constructor { get; }

        internal DyObject[] Locals { get; }

        internal DyObject Value => Locals.Length > 0 ? Locals[^1] : DyNil.Instance;

        internal DyCustomType(int typeCode, string ctor, DyObject[] locals, Unit unit) : base(typeCode) =>
            (Constructor, Locals, DeclaringUnit) = (ctor, locals, unit);

        public override object ToObject() => this;

        internal override DyObject Unbox() => this;

        internal override int GetCount() => Value.GetCount();

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            Value.SetItem(index, value, ctx);

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx) => Value.GetItem(index, ctx);

        protected internal override bool TryGetItem(DyObject index, ExecutionContext ctx, out DyObject value) =>
            Value.TryGetItem(index, ctx, out value);

        protected internal override bool HasItem(string name, ExecutionContext ctx) => Value.HasItem(name, ctx);

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
            if (TypeName == cust.Constructor && ReferenceEquals(cust.Value, DyNil.Instance))
                return new DyString($"{TypeName}()");
            else if (TypeName == cust.Constructor)
                return new DyString($"{TypeName}({cust.Value.ToString(ctx)})");
            else if (ReferenceEquals(cust.Value, DyNil.Instance))
                return new DyString($"{TypeName}.{cust.Constructor}()");
            else
                return new DyString($"{TypeName}.{cust.Constructor}({cust.Value.ToString(ctx)})");
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            if (autoGenMethods)
                return DyInteger.Get(arg.GetCount());

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
