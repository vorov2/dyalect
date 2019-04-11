namespace Dyalect.Runtime.Types
{
    public sealed class DyTag : DyObject
    {
        public string Tag { get; }

        public DyObject Value { get; }

        public DyTag(string tag, DyObject value) : base(StandardType.Tag)
        {
            Tag = tag;
            Value = value;
        }

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(this, obj);

        public override bool AsBool() => Value.AsBool();

        public override long AsInteger() => Value.AsInteger();

        public override double AsFloat() => Value.AsFloat();

        public override object AsObject() => Value.AsObject();

        public override string AsString() => Value.AsString();

        public override DyTypeInfo GetTypeInfo() => DyTagTypeInfo.Instance;
    }
}
