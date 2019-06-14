namespace Dyalect.Runtime.Types
{
    public sealed class DyCustomType : DyObject
    {
        internal DyObject Value { get; }

        internal DyCustomType(int typeCode, DyObject value) : base(typeCode)
        {
            Value = value;
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
    }
}
