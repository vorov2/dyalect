namespace Dyalect.Runtime.Types
{
    internal sealed class DyModuleTypeInfo : DyTypeInfo
    {
        public static readonly DyModuleTypeInfo Instance = new DyModuleTypeInfo();

        private DyModuleTypeInfo() : base(StandardType.Nil)
        {

        }

        public override string TypeName => StandardType.ModuleName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);
    }
}
