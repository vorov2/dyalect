using System;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public class DyNil : DyObject
    {
        private sealed class DyTerminator : DyNil { }

        public static readonly DyNil Instance = new();
        internal static readonly DyNil Terminator = new DyTerminator();

        private DyNil() : base(Type) { }

        public override object ToObject() => this;

        protected internal override bool GetBool() => false;

        public override string ToString() => "nil";

        public override DyObject Clone() => this;

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            ctx.IndexOutOfRange();

        internal override void Serialize(BinaryWriter writer) => writer.Write((int)Type.TypeCode);

        public override int GetHashCode() => HashCode.Combine(Instance);
    }
}
