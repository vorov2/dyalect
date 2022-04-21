using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeDeltaTypeInfo : DyForeignTypeInfo
    {
        public override string TypeName => "TimeDelta";

        #region Standard operations
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Add | SupportedOperations.Sub | SupportedOperations.Get
            | SupportedOperations.Gt | SupportedOperations.Gte | SupportedOperations.Lt
            | SupportedOperations.Lte | SupportedOperations.Eq | SupportedOperations.Neq;

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
        {
            if (format.IsNil())
                return new DyString(arg.ToString());

            if (format.NotString(ctx)) return Default();

            try
            {
                return new DyString(((DyTimeDelta)arg).ToString(format.GetString()));
            }
            catch (FormatException)
            {
                return ctx.ParsingFailed();
            }
        }

        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx)
        {
            return ((DyTimeDelta)arg).Negate();
        }

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return new DyTimeDelta(this, ((DyTimeDelta)left).TotalTicks + ((DyTimeDelta)right).TotalTicks);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            try
            {
                return new DyTimeDelta(this, ((DyTimeDelta)left).TotalTicks - ((DyTimeDelta)right).TotalTicks);
            }
            catch (OverflowException)
            {
                return ctx.Overflow();
            }
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return DyBool.False;

            return ((DyTimeDelta)left).TotalTicks == ((DyTimeDelta)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return DyBool.True ;

            return ((DyTimeDelta)left).TotalTicks != ((DyTimeDelta)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyTimeDelta)left).TotalTicks > ((DyTimeDelta)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyTimeDelta)left).TotalTicks < ((DyTimeDelta)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyTimeDelta)left).TotalTicks >= ((DyTimeDelta)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyTimeDelta)left).TotalTicks <= ((DyTimeDelta)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => DyInteger.Get(((DyTimeDelta)self).TotalTicks),
                _ => base.CastOp(self, targetType, ctx)
            };
        #endregion

        private DyObject Parse(ExecutionContext ctx, DyObject input, DyObject format)
        {
            if (input.NotString(ctx)) return Default();
            if (format.NotString(ctx)) return Default();

            try
            {
                return DyTimeDelta.Parse(this, format.GetString(), input.GetString());
            }
            catch (FormatException)
            {
                return ctx.ParsingFailed();
            }
            catch (OverflowException)
            {
                return ctx.Overflow();
            }
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToString" => Func.Member(name, ToStringWithFormat, -1, new Par("format", DyNil.Instance)),
                "Days" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Days)),
                "Hours" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Hours)),
                "Minutes" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Minutes)),
                "Seconds" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Seconds)),
                "Milliseconds" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Milliseconds)),
                "Ticks" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Ticks)),
                "TotalTicks" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).TotalTicks)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx, DyObject days, DyObject hours, DyObject minutes, DyObject sec, DyObject ms)
        {
            if (days.NotNat(ctx)) return Default();
            if (hours.NotNat(ctx)) return Default();
            if (minutes.NotNat(ctx)) return Default();
            if (sec.NotNat(ctx)) return Default();
            if (ms.NotNat(ctx)) return Default();

            var totalTicks =
                days.GetInteger() * TimeSpan.TicksPerDay
                + hours.GetInteger() * TimeSpan.TicksPerHour
                + minutes.GetInteger() * TimeSpan.TicksPerMinute
                + sec.GetInteger() * TimeSpan.TicksPerSecond
                + ms.GetInteger() * TimeSpan.TicksPerMillisecond;

            return new DyTimeDelta(this, totalTicks);
        }

        private DyObject FromTicks(ExecutionContext ctx, DyObject ticks)
        {
            if (ticks.NotNat(ctx)) return Default();
            return new DyTimeDelta(this, ticks.GetInteger());
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "TimeDelta" => Func.Static(name, New, -1, new Par("days", DyInteger.Zero), new Par("hours", DyInteger.Zero),
                    new Par("minutes", DyInteger.Zero), new Par("seconds", DyInteger.Zero), new Par("milliseconds", DyInteger.Zero),
                    new Par("ticks", DyInteger.Zero)),
                Method.FromTicks => Func.Static(name, FromTicks, -1, new Par("value")),
                Method.Min => Func.Static(name, _ => new DyTimeDelta(this, TimeSpan.MinValue.Ticks)),
                Method.Max => Func.Static(name, _ => new DyTimeDelta(this, TimeSpan.MaxValue.Ticks)),
                Method.Default => Func.Static(name, _ => new DyTimeDelta(this, TimeSpan.Zero.Ticks)),
                Method.Parse => Func.Static(name, Parse, -1, new Par("input"), new Par("format")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
