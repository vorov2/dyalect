namespace Dyalect.Runtime.Types
{
    public sealed class DyBool : DyObject
    {
        public static readonly DyBool True = new DyBool(true);
        public static readonly DyBool False = new DyBool(false);

        private readonly bool value;

        private DyBool(bool value) : base(StandardType.Bool)
        {
            this.value = value;
        }

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(this, obj);

        public override bool AsBool() => value;

        public override long AsInteger() => value ? 1 : 0;

        public override double AsFloat() => value ? 1d : .0d;

        public override object AsObject() => value;

        public override string AsString() => value ? "true" : "false";
    }
}
