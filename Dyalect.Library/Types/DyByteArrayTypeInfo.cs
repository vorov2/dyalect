using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Linq;

namespace Dyalect.Library.Types
{
    public sealed class DyByteArrayTypeInfo : ForeignTypeInfo
    {
        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var buffer = ((DyByteArray)arg).GetBytes();
            var strs = buffer.Select(b => "0x" + b.ToString("X").PadLeft(2, '0')).ToArray();
            return new DyString("ByteArray [" + string.Join(",", strs) + "]");
        }

        public override string TypeName => "ByteArray";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            DyInteger.Get(((DyByteArray)arg).Count);

        private DyObject Read(ExecutionContext ctx, DyObject self, DyObject obj)
        {
            if (obj.TypeId is not DyType.TypeInfo)
                return ctx.InvalidType(obj);

            var type = (DyTypeInfo)obj;
            var bar = (DyByteArray)self;
            return bar.Read(ctx, type);
        }

        private DyObject Write(ExecutionContext ctx, DyObject self, DyObject obj)
        {
            var bar = (DyByteArray)self;
            bar.Write(ctx, obj);
            return DyNil.Instance;
        }

        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "read" => Func.Member(name, Read, -1, new Par("ofType")),
                "write" => Func.Member(name, Write, -1, new Par("value")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Concat(ExecutionContext ctx, DyObject fst, DyObject snd)
        {
            if (fst.TypeId != TypeCode)
                return ctx.InvalidType(fst);

            if (snd.TypeId != TypeCode)
                return ctx.InvalidType(snd);

            var a1 = ((DyByteArray)fst).GetBytes();
            var a2 = ((DyByteArray)snd).GetBytes();
            var a3 = new byte[a1.Length + a2.Length];
            Array.Copy(a1, a3, a1.Length);
            Array.Copy(a2, 0, a3, a1.Length, a2.Length);
            return new DyByteArray(ctx.RuntimeContext, DeclaringUnit, a3);
        }

        private DyObject New(ExecutionContext ctx, DyObject arg)
        {
            var arr = ((DyTuple)arg).Values.Select(o => o.ToObject()).Select(Convert.ToByte).ToArray();
            return new DyByteArray(ctx.RuntimeContext, DeclaringUnit, arr);
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "ByteArray")
                return Func.Static(name, New, 0, new Par("values"));
            if (name == "concat")
                return Func.Static(name, Concat, -1, new Par("fst"), new Par("snd"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
