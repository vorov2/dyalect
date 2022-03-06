using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyChar : DyObject
    {
        private readonly char value;

        public DyChar(DyTypeInfo typeInfo, char value) : base(typeInfo) => this.value = value;

        public override object ToObject() => GetChar();

        protected internal override char GetChar() => value;

        protected internal override string GetString() => value.ToString();

        public override string ToString() => GetString();

        public override DyObject Clone() => this;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write((int)DecType.TypeCode);
            writer.Write(value);
        }

        public override int GetHashCode() => value.GetHashCode();
    }
}
