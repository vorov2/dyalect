using System.Numerics;

namespace Dyalect.Runtime.Types
{
    public sealed class DyInteger : DyObject
    {
        public static readonly DyInteger Zero = new DyInteger(0L);
        public static readonly DyInteger One = new DyInteger(1L);
        public static readonly DyInteger Max = new DyInteger(long.MaxValue);
        public static readonly DyInteger Min = new DyInteger(long.MinValue);

        private readonly long value;

        public DyInteger(long value) : base(StandardType.Integer)
        {
            this.value = value;
        }

        public override int GetHashCode() => value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is DyInteger i)
                return value == i.value;
            else
                return false;
        }

        protected override bool TestEquality(DyObject obj) => value == obj.AsInteger();

        public override object AsObject() => value;

        public override string AsString() => value.ToString(CI.NumberFormat);

        public override bool AsBool() => value != 0;

        public override double AsFloat() => value;

        public override long AsInteger() => value;
    }
}
