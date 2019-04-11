namespace Dyalect.Runtime.Types
{
    public sealed class DyFloat : DyObject
    {
        public static readonly DyFloat Zero = new DyFloat(0D);
        public static readonly DyFloat One = new DyFloat(1D);
        public static readonly DyFloat NaN = new DyFloat(double.NaN);
        public static readonly DyFloat PositiveInfinity = new DyFloat(double.PositiveInfinity);
        public static readonly DyFloat NegativeInfinity = new DyFloat(double.NegativeInfinity);
        public static readonly DyFloat Epsilon = new DyFloat(double.Epsilon);
        public static readonly DyFloat Min = new DyFloat(double.MinValue);
        public static readonly DyFloat Max = new DyFloat(double.MaxValue);

        private readonly double value;

        public DyFloat(double value) : base(StandardType.Float)
        {
            this.value = value;
        }

        public override int GetHashCode() => value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is DyFloat f)
                return value == f.value;
            else
                return false;
        }

        protected override bool TestEquality(DyObject obj) => value == obj.AsFloat();

        public override object AsObject() => value;

        public override string AsString() => value.ToString(CI.NumberFormat);

        public override double AsFloat() => value;

        public override long AsInteger() => (long)value;

        public override bool AsBool() => value > .00001d;

        public override DyTypeInfo GetTypeInfo() => DyFloatTypeInfo.Instance;
    }
}
