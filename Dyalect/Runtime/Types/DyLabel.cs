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

        public override bool AsBool() => Value.AsBool();

        public override long AsInteger() => Value.AsInteger();

        public override double AsFloat() => Value.AsFloat();

        public override object AsObject() => Value.AsObject();

        public override string AsString() => Value.AsString();
    }
}
