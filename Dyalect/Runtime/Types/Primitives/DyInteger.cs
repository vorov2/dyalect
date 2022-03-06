using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyInteger : DyObject
    {
        private readonly long value;

        public DyInteger(DyTypeInfo typeInfo, long value) : base(typeInfo) =>
            this.value = value;

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
            writer.Write((int)DecType.TypeCode);
            writer.Write(value);
        }
    }
}
