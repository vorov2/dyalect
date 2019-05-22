namespace Dyalect.Runtime.Types
{
    public sealed class DyLabel : DyObject
    {
        public string Label { get; }

        public DyObject Value { get; internal set; }

        public DyLabel(string label, DyObject value) : base(DyType.Label)
        {
            Label = label;
            Value = value;
        }

        protected internal override bool GetBool() => Value.GetBool();

        public override object ToObject() => Value.ToObject();

        protected internal override string GetLabel() => Label;

        protected internal override DyObject GetTaggedValue() => Value;
    }

    internal sealed class DyLabelTypeInfo : DyTypeInfo
    {
        public DyLabelTypeInfo() : base(DyType.Label, false)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Label;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var lab = (DyLabel)arg;
            return (DyString)(lab.Label + " " + lab.Value.ToString(ctx).Value);
        }
    }
}
