namespace Dyalect.Runtime.Types
{
    internal sealed class DyLabelTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Label;

        public override int ReflectedTypeId => DyType.Label;

        protected override DyObject ContainsOp(DyObject self, string field, ExecutionContext ctx) =>
            self.GetLabel() == field ? DyBool.True : DyBool.False;

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
        {
            var lab = (DyLabel)arg;
            return new DyString(lab.Label + ": " + lab.Value.ToString(ctx).Value);
        }
    }
}
