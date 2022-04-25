using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyLocalDateTimeTypeInfo : AbstractDateTimeTypeInfo<DyDateTime>
    {
        private const string LocalDateTime = "LocalDateTime";

        public DyTimeDeltaTypeInfo TypeDeltaTypeInfo => DeclaringUnit.TimeDelta;

        public DyLocalDateTimeTypeInfo() : base(LocalDateTime) { }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DyLocalDateTime)left;

            if (right is DyLocalDateTime dt)
                try
                {
                    if (!self.Offset.Equals(dt.Offset))
                        return ctx.InvalidValue(right);

                    return new DyTimeDelta(DeclaringUnit.TimeDelta, self.Ticks - dt.Ticks);
                }
                catch (Exception)
                {
                    return ctx.InvalidValue(right);
                }
            else if (right is DyTimeDelta td)
                try
                {
                    return new DyLocalDateTime(this, self.Ticks - td.TotalTicks, self.Offset);
                }
                catch (Exception)
                {
                    return ctx.InvalidValue(right);
                }

            return ctx.InvalidType(DeclaringUnit.LocalDateTime.TypeId, DeclaringUnit.TimeDelta.TypeId, right);
        }

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DyLocalDateTime)left;
            
            if (right is DyTimeDelta td)
            {
                try
                {
                    if (self.Offset.Ticks != td.TotalTicks)
                        return ctx.InvalidValue(right);

                    return new DyLocalDateTime(this, self.Ticks + td.TotalTicks, self.Offset);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return ctx.InvalidValue(right);
                }
            }

            return ctx.InvalidType(DeclaringUnit.TimeDelta.TypeId, right);
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Offset" => Func.Auto(name, _ => ((DyLocalDateTime)self).Offset),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyTimeDelta? TryGetOffset(ExecutionContext ctx, DyObject offset)
        {
            if (offset.IsNil())
                return new DyTimeDelta(DeclaringUnit.TimeDelta, TimeZoneInfo.Local.BaseUtcOffset.Ticks);
            else if (offset.TypeId != DeclaringUnit.TimeDelta.ReflectedTypeId)
            {
                ctx.InvalidType(DeclaringUnit.TimeDelta.ReflectedTypeId, offset);
                return null;
            }
            else
                return (DyTimeDelta)offset;
        }

        protected override DyObject Parse(string format, string input) => DyLocalDateTime.Parse(this, format, input);

        protected override DyObject Create(long ticks, DyTimeDelta? offset) => new DyLocalDateTime(this, ticks, offset!);
        
        private DyObject CreateNew(ExecutionContext ctx, DyObject year, DyObject month, DyObject day, DyObject hour, DyObject minute, DyObject second, DyObject millisecond,
            DyObject offset)
        {
            var delta = TryGetOffset(ctx, offset);

            if (delta is null)
                return Nil;

            return New(ctx, year, month, day, hour, minute, second, millisecond, delta);
        }

        private DyObject FromTicks(ExecutionContext ctx, DyObject ticks, DyObject offset)
        {
            if (ticks.NotInteger(ctx)) return Nil;
            var delta = TryGetOffset(ctx, offset);

            if (delta is null)
                return Nil;

            return new DyLocalDateTime(this, ticks.GetInteger(), delta);
        }

        private bool CheckValues(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            if (dateTime.TypeId != DeclaringUnit.DateTime.ReflectedTypeId)
            {
                ctx.InvalidType(DeclaringUnit.DateTime.ReflectedTypeId, dateTime);
                return false;
            }

            if (timeZone.NotNil() && timeZone.TypeId != DeclaringUnit.TimeDelta.ReflectedTypeId)
            {
                ctx.InvalidType(DeclaringUnit.TimeDelta.ReflectedTypeId, timeZone);
                return false;
            }

            return true;
        }

        private DyObject GetLocalDateTime(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            if (!CheckValues(ctx, dateTime, timeZone)) return Nil;
            var (dt, td) = (((DyDateTime)dateTime).ToDateTime(), ((DyTimeDelta)timeZone).ToTimeSpan());
            var targetZone = TimeZoneInfo.CreateCustomTimeZone(Guid.NewGuid().ToString(), td, null, null);
            var tzz = timeZone.NotNil() ? targetZone : TimeZoneInfo.Local;
            var dat = TimeZoneInfo.ConvertTimeFromUtc(dt, tzz);
            return new DyLocalDateTime(DeclaringUnit.LocalDateTime, dat.Ticks,
                new DyTimeDelta(DeclaringUnit.TimeDelta, tzz.BaseUtcOffset));
        }

        protected override DyDateTime GetMax(ExecutionContext ctx) => new(this, DateTime.MaxValue.Ticks);

        protected override DyDateTime GetMin(ExecutionContext ctx) => new(this, DateTime.MinValue.Ticks);

        private DyTimeDelta LocalOffset() => new(DeclaringUnit.TimeDelta, TimeZoneInfo.Local.BaseUtcOffset);

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                LocalDateTime => Func.Static(name, CreateNew, -1, new Par("year"), new Par("month"), new Par("day"),
                    new Par("hour", DyInteger.Zero), new Par("minute", DyInteger.Zero),
                    new Par("second", DyInteger.Zero), new Par("millisecond", DyInteger.Zero), new Par("offset", DyNil.Instance)),
                "FromDateTime" => Func.Static(name, GetLocalDateTime, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "FromTicks" => Func.Static(name, FromTicks, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "Now" => Func.Static(name, _ => new DyLocalDateTime(this, DateTime.Now.Ticks, LocalOffset())),
                "LocalOffset" => Func.Auto(name, _ => LocalOffset()),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
