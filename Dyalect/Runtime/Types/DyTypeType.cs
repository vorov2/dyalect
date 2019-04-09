namespace Dyalect.Runtime.Types
{
    internal sealed class DyTypeType : DyType
    {
        public static readonly DyTypeType Instance = new DyTypeType();

        private DyTypeType() : base(StandardType.Type)
        {

        }

        public override string TypeName => StandardType.TypeName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);
    }
}
