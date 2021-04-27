using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyCustomType : DyObject
    {
        internal DyObject Value { get; }

        internal Unit DeclaringUnit { get; }

        internal string Constructor { get; }

        internal DyCustomType(int typeCode, string ctor, DyObject value, Unit unit) : base(typeCode) =>
            (Value, Constructor, DeclaringUnit) = (value, ctor, unit);

        public override object ToObject() => Value.ToObject();

        internal override DyObject Unbox() => Value;

        internal override int GetCount() => Value.GetCount();

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            Value.SetItem(index, value, ctx);

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx) => Value.GetItem(index, ctx);

        protected internal override bool TryGetItem(DyObject index, ExecutionContext ctx, out DyObject value) =>
            Value.TryGetItem(index, ctx, out value);

        protected internal override bool HasItem(string name, ExecutionContext ctx) => Value.HasItem(name, ctx);

        public override string GetConstructor(ExecutionContext ctx) => Constructor;

        public override int GetHashCode() => HashCode.Combine(Constructor, Value);

        public override bool Equals(DyObject other) =>
            other is DyCustomType ct
            && ct.Constructor == Constructor
            && (ReferenceEquals(ct.Value, Value) || (ct.Value is not null && ct.Value.Equals(Value)));
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
