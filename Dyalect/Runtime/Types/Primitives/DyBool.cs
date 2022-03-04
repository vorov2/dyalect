using System.IO;
using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    public abstract class DyBool : DyObject
    {
        internal static readonly DyBoolTypeInfo Type = new();

        public static readonly DyBool True = new TrueBool();
        public static readonly DyBool False = new FalseBool();

        private sealed class TrueBool : DyBool
        {
            protected internal override bool GetBool() => true;

            public override string ToString() => "true";

            public override int GetHashCode() => true.GetHashCode();
        }

        private sealed class FalseBool : DyBool
        {
            protected internal override bool GetBool() => false;

            public override string ToString() => "false";

            public override int GetHashCode() => false.GetHashCode();
        }

        private DyBool() : base(Type) { }

        public override object ToObject() => GetBool();

        public override DyObject Clone() => this;

        public static explicit operator DyBool(bool v) => v ? True : False;

        public static explicit operator bool(DyBool v) => ReferenceEquals(v, True);

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write((int)Type.TypeCode);
            writer.Write(this is TrueBool);
        }
    }
}
