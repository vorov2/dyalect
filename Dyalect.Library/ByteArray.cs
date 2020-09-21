using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Linq;

namespace Dyalect.Library
{
    public sealed class ByteArray : DyObject
    {
        internal readonly byte[] Buffer;

        public ByteArray(int typeCode, byte[] buffer) : base(typeCode)
        {
            this.Buffer = buffer;
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
            return new DyString("ByteArray[" + string.Join(",", ((ByteArray)arg).Buffer) + "]");
        }

        public override string TypeName => nameof(ByteArray);

        protected override SupportedOperations GetSupportedOperations()
        {
            throw new NotImplementedException();
        }

        private DyObject Concat(ExecutionContext ctx, DyObject fst, DyObject snd)
        {
            if (fst.TypeId != TypeCode)
                return ctx.InvalidType(fst);

            if (snd.TypeId != TypeCode)
                return ctx.InvalidType(snd);

            var a1 = ((ByteArray)fst).Buffer;
            var a2 = ((ByteArray)snd).Buffer;
            var a3 = new byte[a1.Length + a2.Length];
            Array.Copy(a1, a3, a1.Length);
            Array.Copy(a2, 0, a3, a1.Length, a2.Length);
            return new ByteArray(TypeCode, a3);
        }

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            var vals = DyIterator.Run(ctx, arg);
            var arr =  vals.Select(o => o.ToObject()).Select(Convert.ToByte).ToArray();
            return new ByteArray(TypeCode, arr);
        }

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "ByteArray")
                return DyForeignFunction.Static(name, New, -1, new Par("values"));
            if (name == "concat")
                return DyForeignFunction.Static(name, Concat, -1, new Par("fst"), new Par("snd"));

            return base.GetStaticMember(name, ctx);
        }
    }
}
