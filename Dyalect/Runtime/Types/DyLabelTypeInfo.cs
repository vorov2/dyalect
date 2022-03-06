namespace Dyalect.Runtime.Types
{
    internal sealed class DyLabelTypeInfo : DyTypeInfo
    {
        public DyLabelTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Label) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Label;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var lab = (DyLabel)arg;
            return new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, lab.Label + ": " + lab.Value.ToString(ctx).Value);
        }
    }
}
