namespace Dyalect.Runtime.Types
{
    internal sealed class DyTagTypeInfo : DyTypeInfo
    {
        public static readonly DyTagTypeInfo Instance = new DyTagTypeInfo();

        private DyTagTypeInfo() : base(StandardType.Tag)
        {

        }

        public override string TypeName => StandardType.TagName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);
    }
}
