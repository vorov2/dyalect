using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyFloat : DyObject
    {
        private readonly double value;

        public DyFloat(DyTypeInfo typeInfo, double value) : base(typeInfo) =>
            this.value = value;

        public override int GetHashCode() => value.GetHashCode();

        public override bool Equals(DyObject? obj) => obj is DyFloat f && value == f.value;

        public override string ToString() => value.ToString(CI.NumberFormat);

        public override object ToObject() => value;

        protected internal override double GetFloat() => value;

        protected internal override long GetInteger() => (long)value;

        protected internal override bool GetBool() => value > .00001d;

        public override DyObject Clone() => this;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write((int)DecType.TypeCode);
            writer.Write(value);
        }
    }
}
