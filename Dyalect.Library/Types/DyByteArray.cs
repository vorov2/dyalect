using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Linq;

namespace Dyalect.Library.Types
{
    public sealed class DyByteArray : DyForeignObject<DyByteArrayTypeInfo>
    {
        internal byte[] Buffer;

        public DyByteArray(RuntimeContext rtx, Unit unit, byte[] buffer) : base(rtx, unit) => Buffer = buffer;

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
            return new DyByteArray(ctx.RuntimeContext, DeclaringUnit, a3);
        }

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            var vals = DyIterator.ToEnumerable(ctx, arg);
            var arr =  vals.Select(o => o.ToObject()).Select(Convert.ToByte).ToArray();
            return new DyByteArray(ctx.RuntimeContext, DeclaringUnit, arr);
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "ByteArray")
                return Func.Static(name, New, -1, new Par("values"));
            if (name == "concat")
                return Func.Static(name, Concat, -1, new Par("fst"), new Par("snd"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
