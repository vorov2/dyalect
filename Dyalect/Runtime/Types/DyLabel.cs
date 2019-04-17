namespace Dyalect.Runtime.Types
{
    public sealed class DyLabel : DyObject
    {
        public string Label { get; }

        public DyObject Value { get; }

        public DyLabel(string label, DyObject value) : base(StandardType.Label)
        {
            Label = label;
            Value = value;
        }

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(this, obj);

        protected internal override bool GetBool() => Value.GetBool();

        public override object ToObject() => Value.ToObject();
    }

    internal sealed class DyLabelTypeInfo : DyTypeInfo
    {
        public static readonly DyLabelTypeInfo Instance = new DyLabelTypeInfo();

        private DyLabelTypeInfo() : base(StandardType.Label)
        {

        }

        public override string TypeName => StandardType.LabelName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var lab = (DyLabel)arg;
            return lab.Label + " " + lab.Value.ToString(ctx).Value;
        }
    }
}
