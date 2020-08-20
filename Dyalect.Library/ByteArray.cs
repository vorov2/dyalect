using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library
{
    public sealed class ByteArray : DyObject
    {
        public ByteArray(int typeCode) : base(typeCode)
        {

        }

        public override object ToObject()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ByteArrayTypeInfo : DyTypeInfo
    {
        public ByteArrayTypeInfo(int typeCode) : base(typeCode)
        {

        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            return new DyString("ByteArray!");
        }

        public override string TypeName => nameof(ByteArray);

        protected override SupportedOperations GetSupportedOperations()
        {
            throw new NotImplementedException();
        }
    }
}
