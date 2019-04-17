namespace Dyalect.Runtime.Types
{
    public sealed class DyNil : DyObject
    {
        public static readonly DyNil Instance = new DyNil();

        private DyNil() : base(StandardType.Nil)
        {
            
        }

        public override object ToObject() => null;

        protected internal override bool GetBool() => false;

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(this, obj);
    }

    internal sealed class DyNilTypeInfo : DyTypeInfo
    {
        public static readonly DyNilTypeInfo Instance = new DyNilTypeInfo();

        private DyNilTypeInfo() : base(StandardType.Nil)
        {

        }

        public override string TypeName => StandardType.NilName;

        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => DyBool.True;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString("nil");
    }
}
