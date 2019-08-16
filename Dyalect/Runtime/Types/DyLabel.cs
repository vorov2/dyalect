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

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            (index.TypeId == DyType.Integer && index.GetInteger() == 0) ||
                (index.TypeId == DyType.String && index.GetString() == Label)
                ? Value : ctx.IndexOutOfRange(this.TypeName(ctx), index);

        internal protected override DyObject GetItem(string name, ExecutionContext ctx) =>
            name == Label ? Value : ctx.IndexOutOfRange(this.TypeName(ctx), name);

        internal protected override DyObject GetItem(int index, ExecutionContext ctx) =>
            index == 0 ? Value : ctx.IndexOutOfRange(this.TypeName(ctx), index);

        protected internal override bool HasItem(string name, ExecutionContext ctx) => name == Label;

        protected internal override bool TryGetItem(string name, ExecutionContext ctx, out DyObject value)
        {
            if (name == Label)
            {
                value = Value;
                return true;
            }

            value = null;
            return false;
        }

        protected internal override bool TryGetItem(int index, ExecutionContext ctx, out DyObject value)
        {
            if (index != 0)
            {
                value = null;
                return false;
            }

            value = Value;
            return true;
        }
    }

    internal sealed class DyLabelTypeInfo : DyTypeInfo
    {
        public DyLabelTypeInfo() : base(DyType.Label)
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
