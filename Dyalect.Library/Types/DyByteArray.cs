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
        internal byte[] Buffer;

        public DyByteArray(RuntimeContext rtx, byte[] buffer) : base(rtx) => Buffer = buffer;

        public override object ToObject() => Buffer;

        public override DyObject Clone()
        {
            var clone = (DyByteArray)MemberwiseClone();
            clone.Buffer = (byte[])Buffer.Clone();
            return clone;
        }
    }

    public sealed class DyByteArrayTypeInfo : ForeignTypeInfo
    {
        public DyByteArrayTypeInfo() { }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString("ByteArray [" + string.Join(",", ((DyByteArray)arg).Buffer) + "]");

        public override string TypeName => "ByteArray";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            DyInteger.Get(((DyByteArray)arg).Buffer.Length);

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
            return new DyByteArray(ctx.RuntimeContext, a3);
        }

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            var vals = DyIterator.Run(ctx, arg);
            var arr =  vals.Select(o => o.ToObject()).Select(Convert.ToByte).ToArray();
            return new DyByteArray(ctx.RuntimeContext, arr);
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
