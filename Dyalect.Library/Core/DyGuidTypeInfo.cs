using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyGuidTypeInfo : DyForeignTypeInfo<CoreModule>
    {
        public override string TypeName => "Guid";

        public DyByteArray Create(byte[]? buffer) => new(this, buffer);

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString(arg.ToString());

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        private DyObject ToByteArray(ExecutionContext _, DyObject self)
        {
            var g = (DyGuid)self;
            return new DyByteArray(DeclaringUnit.ByteArray, g.Value.ToByteArray());
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return DyBool.False;

            return ((DyGuid)left).Value == ((DyGuid)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToByteArray" => Func.Member(name, ToByteArray),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Parse(ExecutionContext ctx, DyObject arg)
        {
            if (arg.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, arg);

            try
            {
                return new DyGuid(this, Guid.Parse(arg.GetString()));
            }
            catch (FormatException)
            {
                return ctx.InvalidValue(arg);
            }
        }

        private DyObject FromByteArray(ExecutionContext ctx, DyObject arg)
        {
            if (arg is not DyByteArray arr)
                return ctx.InvalidType(DeclaringUnit.ByteArray.ReflectedTypeId, arg);

            try
            {
                return new DyGuid(this, new Guid(arr.GetBytes()));
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(arg);
            }
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Guid" => Func.Static(name, _ => new DyGuid(this, Guid.NewGuid())),
                "Empty" => Func.Static(name, _ => new DyGuid(this, Guid.Empty)),
                "Parse" => Func.Static(name, Parse, -1, new Par("value")),
                "FromByteArray" => Func.Static(name, FromByteArray, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx)
        {
            if (targetType.ReflectedTypeId == DyType.String)
                return self.ToString(ctx);
            else if (targetType.ReflectedTypeId == DeclaringUnit.ByteArray.ReflectedTypeId)
                return ToByteArray(ctx, self);
            else
                return base.CastOp(self, targetType, ctx);
        }
    }
}
