namespace Dyalect.Runtime.Types
{
    public sealed class DyLabel : DyObject
    {
        public string Label { get; }

        public DyObject Value { get; internal set; }

        public DyLabel(string label, DyObject value) : base(StandardType.Label)
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
        public DyLabelTypeInfo() : base(StandardType.Label, false)
        {

        }

        public override string TypeName => StandardType.LabelName;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var lab = (DyLabel)arg;
            return lab.Label + " " + lab.Value.ToString(ctx).Value;
        }
    }
}
