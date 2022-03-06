using System;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public class DyNil : DyObject
    {
        internal sealed class Terminator : DyNil
        {
            public Terminator(DyTypeInfo typeInfo) : base(typeInfo) 
            {

            }
        }

        internal DyNil(DyTypeInfo typeInfo) : base(typeInfo) { }

        public override object ToObject() => this;

        protected internal override bool GetBool() => false;

        public override string ToString() => "nil";

        public override DyObject Clone() => this;

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            ctx.IndexOutOfRange();

        internal override void Serialize(BinaryWriter writer) => writer.Write((int)DecType.TypeCode);

        public override int GetHashCode() => HashCode.Combine(DecType.TypeName, DecType.TypeCode);
    }
}
