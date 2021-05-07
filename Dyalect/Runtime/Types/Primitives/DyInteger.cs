using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyInteger : DyObject
    {
        public static readonly DyInteger Zero = new(0L);
        public static readonly DyInteger MinusOne = new(-1L);
        public static readonly DyInteger One = new(1L);
        public static readonly DyInteger Two = new(2L);
        public static readonly DyInteger Three = new(3L);
        public static readonly DyInteger Max = new(long.MaxValue);
        public static readonly DyInteger Min = new(long.MinValue);

        private readonly long value;

        public DyInteger(long value) : base(DyType.Integer) =>
            this.value = value;

        public static DyInteger Get(long i) =>
            i switch
            {
                -1 => MinusOne,
                0 => Zero,
                1 => One,
                2 => Two,
                3 => Three,
                _ => new DyInteger(i)
            };

        public override int GetHashCode() => value.GetHashCode();

        public override string ToString() => value.ToString(CI.Default);

        public override bool Equals(DyObject? obj) => obj is DyInteger i && value == i.value;

        public override object ToObject() => value;

        protected internal override bool GetBool() => value != 0;

        protected internal override double GetFloat() => value;

        protected internal override long GetInteger() => value;

        public override DyObject Clone() => this;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(value);
        }
    }
}
