namespace Dyalect.Runtime.Types
{
    public class DyWrapperTypeInfo : DyTypeInfo
    {
        internal static readonly DyWrapperTypeInfo Instance = new();

        public DyWrapperTypeInfo() : base(DyTypeCode.Object) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get;

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) =>
            self.GetItem(index, ctx);

        public override string TypeName => DyTypeNames.Object;
    }
}
