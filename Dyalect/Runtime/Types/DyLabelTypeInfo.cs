namespace Dyalect.Runtime.Types
{
    internal sealed class DyLabelTypeInfo : DyTypeInfo
    {
        public static readonly DyLabelTypeInfo Instance = new DyLabelTypeInfo();

        private DyLabelTypeInfo() : base(StandardType.Label)
        {

        }

        public override string TypeName => StandardType.LabelName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);
    }
}
