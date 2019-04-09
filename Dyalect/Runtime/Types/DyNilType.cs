namespace Dyalect.Runtime.Types
{
    internal sealed class DyNilType : DyType
    {
        public static readonly DyNilType Instance = new DyNilType();

        private DyNilType() : base(StandardType.Nil)
        {

        }

        public override string TypeName => StandardType.NilName;

        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => DyBool.True;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);
    }
}
