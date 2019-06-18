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

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx) => Value.GetItem(index, ctx);

        protected internal override DyObject GetItem(int index, ExecutionContext ctx) => Value.GetItem(index, ctx);

        protected internal override bool HasItem(string name, ExecutionContext ctx) => Value.HasItem(name, ctx);
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
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

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

        public override string TypeName { get; }

        protected override DyBool HasMemberDirect(DyObject self, string name, int nameId, ExecutionContext ctx)
        {
            switch (name)
            {
                case Builtins.Not:
                case Builtins.ToStr:
                case Builtins.Clone:
                case Builtins.Has:
                    return DyBool.True;
                case Builtins.Len:
                    if (!autoGenMethods)
                        goto default;
                    return DyBool.True;
                case Builtins.Get:
                    if (!autoGenMethods)
                        goto default;
                    return DyBool.True;
                default:
                    return nameId != -1 && CheckHasMemberDirect(self, nameId, ctx) ? DyBool.True : DyBool.False;
            }
        }
    }
}
