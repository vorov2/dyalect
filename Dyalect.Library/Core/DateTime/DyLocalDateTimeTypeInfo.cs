using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyLocalDateTimeTypeInfo : DyBaseDateTimeTypeInfo
    {
        private const string FORMAT = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz";

        public DyLocalDateTimeTypeInfo() : base("LocalDateTime") { }

        protected override DyDateTime CreateDateTime(DateTime dateTime, TimeSpan? offset) =>
            new DyLocalDateTime(this, dateTime, offset!.Value);

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
        {
            if (format.NotNil() && !format.IsString(ctx)) return Default();

            var local = (DyLocalDateTime)arg;
            var formatStr = format.NotNil() ? format.GetString() : FORMAT;

            try
            {
                var dto = new DateTimeOffset(DateTime.SpecifyKind(local.Value, DateTimeKind.Unspecified), local.Offset!.Value);
                var str = dto.ToString(formatStr, CI.Default);
                return new DyString(str);
            }
            catch (Exception)
            {
                return ctx.InvalidValue(format);
            }
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DyLocalDateTime)left;

            if (right is DyLocalDateTime dt)
                try
                {
                    if (self.Offset != dt.Offset)
                        return ctx.InvalidValue(right);

                    return new DyTimeDelta(DeclaringUnit.TimeDelta, self.Value - dt.Value);
                }
                catch (Exception)
                {
                    return ctx.InvalidValue(right);
                }
            else if (right is DyTimeDelta td)
                try
                {
                    return new DyLocalDateTime(this, self.Value - td.Value, self.Offset!.Value);
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
                    if (self.Offset != td.Value)
                        return ctx.InvalidValue(right);

                    return new DyLocalDateTime(this, self.Value + td.Value, self.Offset!.Value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return ctx.InvalidValue(right);
                }
            }

            return ctx.InvalidType(DeclaringUnit.TimeDelta.TypeId, right);
        }

        private DyObject Parse(ExecutionContext ctx, DyObject value, DyObject format)
        {
            if (!value.IsString(ctx)) return Default();
            if (!format.IsString(ctx)) return Default();

            try
            {
                var dt = DateTimeOffset.ParseExact(value.GetString(), format.GetString(), CI.Default);
                return new DyLocalDateTime(this, dt.DateTime, dt.Offset);
            }
            catch (FormatException ex)
            {
                return ctx.ParsingFailed(ex.Message);
            }
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Offset" => Func.Auto(name, _ => new DyTimeDelta(DeclaringUnit.TimeDelta, ((DyDateTime)self).Offset!.Value)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private bool TryGetOffset(ExecutionContext ctx, DyObject offset, out TimeSpan timeSpan)
        {
            timeSpan = default;

            if (!offset.NotNil())
                timeSpan = TimeZoneInfo.Local.BaseUtcOffset;
            else if (offset.TypeId != DeclaringUnit.TimeDelta.ReflectedTypeId)
            {
                ctx.InvalidType(DeclaringUnit.TimeDelta.ReflectedTypeId, offset);
                return false;
            }
            else
                timeSpan = ((DyTimeDelta)offset).Value;

            return true;
        }

        private DyObject New(ExecutionContext ctx, DyObject year, DyObject month, DyObject day, DyObject hour, DyObject minute, DyObject second, DyObject millisecond,
            DyObject offset)
        {
            if (!TryGetOffset(ctx, offset, out var timeSpan))
                return Default();

            return New(ctx, year, month, day, hour, minute, second, millisecond, timeSpan);
        }

        private DyObject FromTicks(ExecutionContext ctx, DyObject ticks, DyObject offset)
        {
            if (!TryGetOffset(ctx, offset, out var timeSpan))
                return Default();

            return FromTicks(ctx, ticks, timeSpan);
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
            if (!CheckValues(ctx, dateTime, timeZone)) return Default();
            var (dt, td) = (((DyDateTime)dateTime).Value, ((DyTimeDelta)timeZone).Value);
            var targetZone = TimeZoneInfo.CreateCustomTimeZone(Guid.NewGuid().ToString(), td, null, null);
            var tzz = timeZone.NotNil() ? targetZone : TimeZoneInfo.Local;
            var dat = TimeZoneInfo.ConvertTimeFromUtc(dt, tzz);
            return new DyLocalDateTime(DeclaringUnit.LocalDateTime, dat, tzz.BaseUtcOffset);
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "FromDateTime" => Func.Static(name, GetLocalDateTime, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "Now" => Func.Static(name, _ => new DyLocalDateTime(this, DateTime.Now, TimeZoneInfo.Local.BaseUtcOffset)),
                "Min" => Func.Static(name, _ => new DyLocalDateTime(this, DateTime.MinValue, TimeZoneInfo.Local.BaseUtcOffset)),
                "Max" => Func.Static(name, _ => new DyLocalDateTime(this, DateTime.MaxValue, TimeZoneInfo.Local.BaseUtcOffset)),
                "Default" => Func.Static(name, _ => new DyLocalDateTime(this, DateTime.MinValue, TimeZoneInfo.Local.BaseUtcOffset)),
                "Parse" => Func.Static(name, Parse, -1, new Par("value"), new Par("format")),
                "LocalDateTime" => Func.Static(name, New, -1, new Par("year", DyInteger.Zero), new Par("month", DyInteger.Zero),
                    new Par("day", DyInteger.Zero), new Par("hour", DyInteger.Zero), new Par("minute", DyInteger.Zero),
                    new Par("second", DyInteger.Zero), new Par("millisecond", DyInteger.Zero), new Par("offset", DyNil.Instance)),
                "FromTicks" => Func.Static(name, FromTicks, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "LocalOffset" => Func.Auto(name, _ => new DyTimeDelta(DeclaringUnit.TimeDelta, TimeZoneInfo.Local.BaseUtcOffset)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
