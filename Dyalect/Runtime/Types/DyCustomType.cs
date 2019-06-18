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
    }

    internal sealed class DyCustomTypeInfo : DyTypeInfo
    {
        public DyCustomTypeInfo(int typeCode, string typeName) : base(typeCode)
        {
            TypeName = typeName;
        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var cust = (DyCustomType)arg;
            return new DyString($"{{{TypeName}: {cust.Value.ToString(ctx)}}}");
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
                default:
                    return nameId != -1 && CheckHasMemberDirect(self, nameId, ctx) ? DyBool.True : DyBool.False;
            }
        }
    }
}
