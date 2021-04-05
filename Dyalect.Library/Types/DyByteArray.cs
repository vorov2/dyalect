using Dyalect.Debug;
using Dyalect.Linker;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Linq;

namespace Dyalect.Library.Types
{
    public sealed class DyByteArray : DyForeignObject<DyByteArrayTypeInfo>
    {
        internal readonly byte[] Buffer;

        public DyByteArray(RuntimeContext rtx, byte[] buffer) : base(rtx)
        {
            this.Buffer = buffer;
        }

        internal DyByteArray(int typeId, byte[] buffer) : base(typeId)
        {
            this.Buffer = buffer;
        }

        public override object ToObject() => Buffer;

        public override DyObject Clone() => new DyByteArray(TypeId, (byte[])Buffer.Clone());
    }

    [ForeignType("8B01A32B-C575-45CC-B6B8-4DA5A14F7206")]
    public sealed class DyByteArrayTypeInfo : ForeignTypeInfo
    {
        public DyByteArrayTypeInfo()
        {

        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            return new DyString("ByteArray [" + string.Join(",", ((DyByteArray)arg).Buffer) + "]");
        }

        public override string TypeName => "ByteArray";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DyByteArray)arg;
            return DyInteger.Get(self.Buffer.Length);
        }

        private DyObject Concat(ExecutionContext ctx, DyObject fst, DyObject snd)
        {
            if (fst.TypeId != TypeCode)
                return ctx.InvalidType(fst);

            if (snd.TypeId != TypeCode)
                return ctx.InvalidType(snd);

            var a1 = ((DyByteArray)fst).Buffer;
            var a2 = ((DyByteArray)snd).Buffer;
            var a3 = new byte[a1.Length + a2.Length];
            Array.Copy(a1, a3, a1.Length);
            Array.Copy(a2, 0, a3, a1.Length, a2.Length);
            return new DyByteArray(TypeCode, a3);
        }

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            var vals = DyIterator.Run(ctx, arg);
            var arr =  vals.Select(o => o.ToObject()).Select(Convert.ToByte).ToArray();
            return new DyByteArray(TypeCode, arr);
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
