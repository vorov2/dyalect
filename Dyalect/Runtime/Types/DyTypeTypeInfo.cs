namespace Dyalect.Runtime.Types
{
    internal sealed class DyTypeTypeInfo : DyTypeInfo
    {
        public static readonly DyTypeTypeInfo Instance = new DyTypeTypeInfo();

        private DyTypeTypeInfo() : base(StandardType.Type)
        {

        }

        public override string TypeName => StandardType.TypeName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);
    }
}
