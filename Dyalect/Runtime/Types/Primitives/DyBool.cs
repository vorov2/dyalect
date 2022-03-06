using System.IO;
using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    public abstract class DyBool : DyObject
    {
        internal sealed class True: DyBool
        {
            public True(DyTypeInfo typeInfo) : base(typeInfo) { }

            protected internal override bool GetBool() => true;

            public override string ToString() => "true";

            public override int GetHashCode() => true.GetHashCode();
        }

        internal sealed class False: DyBool
        {
            public False(DyTypeInfo typeInfo) : base(typeInfo) { }

            protected internal override bool GetBool() => false;

            public override string ToString() => "false";

            public override int GetHashCode() => false.GetHashCode();
        }

        private DyBool(DyTypeInfo typeInfo) : base(typeInfo) { }

        public override object ToObject() => GetBool();

        public override DyObject Clone() => this;

        public static explicit operator bool(DyBool v) => v is True;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write((int)DecType.TypeCode);
            writer.Write(this is True);
        }
    }
}
