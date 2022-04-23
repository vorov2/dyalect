﻿using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyGuidTypeInfo : DyForeignTypeInfo<CoreModule>
    {
        private const string GuidType = "Guid";

        public override string ReflectedTypeName => GuidType;

        public DyGuidTypeInfo() => AddMixin(DyType.Comparable);

        public DyByteArray Create(byte[]? buffer) => new(this, buffer);

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
            new DyString("{" + arg.ToString().ToUpper() + "}");

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

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return (DyBool)(((DyGuid)left).Value.CompareTo(((DyGuid)right).Value) > 0);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return (DyBool)(((DyGuid)left).Value.CompareTo(((DyGuid)right).Value) >= 0);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return (DyBool)(((DyGuid)left).Value.CompareTo(((DyGuid)right).Value) < 0);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return (DyBool)(((DyGuid)left).Value.CompareTo(((DyGuid)right).Value) <= 0);
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToByteArray" => Func.Member(name, ToByteArray),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Parse(ExecutionContext ctx, DyObject arg)
        {
            if (!arg.IsString(ctx)) return Default();

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
                return new DyGuid(this, new(arr.GetBytes()));
            }
            catch (ArgumentException)
            {
                return ctx.InvalidValue(arg);
            }
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                GuidType => Func.Static(name, _ => new DyGuid(this, Guid.NewGuid())),
                "Empty" => Func.Static(name, _ => new DyGuid(this, Guid.Empty)),
                "Parse" => Func.Static(name, Parse, -1, new Par("value")),
                "FromByteArray" => Func.Static(name, FromByteArray, -1, new Par("value")),
                "Default" => Func.Static(name, _ => new DyGuid(this, Guid.Empty)),
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
