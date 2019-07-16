using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public sealed class DyCustomType : DyObject
    {
        internal DyObject Value { get; }

        internal int ConstructorId { get; }

        internal DyCustomType(int typeCode, int ctorId, DyObject value) : base(typeCode)
        {
            Value = value;
            ConstructorId = ctorId;
        }

        public override object ToObject() => Value.ToObject();

        internal override DyObject GetSelf() => Value;

        internal override int GetCount() => Value.GetCount();

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx) => 
            Value.SetItem(index, value, ctx);

        protected internal override void SetItem(int index, DyObject value, ExecutionContext ctx) =>
            Value.SetItem(index, value, ctx);

        protected internal override void SetItem(string name, DyObject value, ExecutionContext ctx) =>
            Value.SetItem(name, value, ctx);

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx) => Value.GetItem(index, ctx);

        protected internal override DyObject GetItem(int index, ExecutionContext ctx) => Value.GetItem(index, ctx);

        protected internal override bool TryGetItem(string name, ExecutionContext ctx, out DyObject value) =>
            Value.TryGetItem(name, ctx, out value);

        protected internal override bool HasItem(string name, ExecutionContext ctx) => Value.HasItem(name, ctx);

        internal override int GetConstructorId(ExecutionContext ctx) => ConstructorId;
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
            | (autoGenMethods ? (SupportedOperations.Get | SupportedOperations.Set) : SupportedOperations.None);

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var cust = (DyCustomType)arg;
            return new DyString($"{{{TypeName}: {cust.Value.ToString(ctx)}}}");
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

        protected override DyObject GetOp(DyObject self, int index, ExecutionContext ctx)
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

        protected override DyObject SetOp(DyObject self, int index, DyObject value, ExecutionContext ctx)
        {
            if (autoGenMethods)
            {
                self.SetItem(index, value, ctx);
                return DyNil.Instance;
            }

            return base.SetOp(self, index, value, ctx);
        }

        public override string TypeName { get; }

        protected override bool HasMemberDirect(DyObject self, string name, int nameId, ExecutionContext ctx)
        {
            switch (name)
            {
                case Builtins.Not:
                case Builtins.ToStr:
                case Builtins.Clone:
                case Builtins.Has:
                    return true;
                case Builtins.Len:
                    if (!autoGenMethods)
                        goto default;
                    return true;
                case Builtins.Get:
                    if (!autoGenMethods)
                        goto default;
                    return true;
                default:
                    return nameId != -1 && CheckHasMemberDirect(self, nameId, ctx);
            }
        }
    }
}
