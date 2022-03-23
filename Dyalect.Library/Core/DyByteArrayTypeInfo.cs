using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Linq;

namespace Dyalect.Library.Core
{
    public sealed class DyByteArrayTypeInfo : DyForeignTypeInfo
    {
        public override string TypeName => "ByteArray";

        public DyByteArray Create(byte[]? buffer) => new(this, buffer);

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var buffer = ((DyByteArray)arg).GetBytes();
            var strs = buffer.Select(b => "0x" + b.ToString("X").PadLeft(2, '0')).ToArray();
            return new DyString("{" + string.Join(",", strs) + "}");
        }

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

        private DyObject GetPosition(ExecutionContext ctx, DyObject self)
        {
            var bar = (DyByteArray)self;
            return DyInteger.Get(bar.Position);
        }

        private DyObject Reset(ExecutionContext ctx, DyObject self)
        {
            var bar = (DyByteArray)self;
            bar.Reset();
            return DyNil.Instance;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Read" => Func.Member(name, Read, -1, new Par("typeInfo")),
                "Write" => Func.Member(name, Write, -1, new Par("value")),
                "Position" => Func.Member(name, GetPosition),
                "Reset" => Func.Member(name, Reset),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Concat(ExecutionContext ctx, DyObject fst, DyObject snd)
        {
            if (fst.TypeId != TypeId)
                return ctx.InvalidType(fst);

            if (snd.TypeId != TypeId)
                return ctx.InvalidType(snd);

            var a1 = ((DyByteArray)fst).GetBytes();
            var a2 = ((DyByteArray)snd).GetBytes();
            var a3 = new byte[a1.Length + a2.Length];
            Array.Copy(a1, a3, a1.Length);
            Array.Copy(a2, 0, a3, a1.Length, a2.Length);
            return new DyByteArray(this, a3);
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "ByteArray" => Func.Static(name, _ => new DyByteArray(this, null)),
                "Concat" => Func.Static(name, Concat, -1, new Par("first"), new Par("second")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
