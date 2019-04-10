namespace Dyalect.Runtime.Types
{
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
    }
}
