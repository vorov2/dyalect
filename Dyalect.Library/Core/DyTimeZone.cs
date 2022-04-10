using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeZoneTypeInfo : DyForeignTypeInfo<CoreModule>
    {
        public override string TypeName => "TimeZone";

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private bool CheckValues(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            if (dateTime.TypeId != DeclaringUnit.DateTime.TypeId)
            {
                ctx.InvalidType(DeclaringUnit.DateTime.TypeId, dateTime);
                return false;
            }

            if (timeZone.NotNil() && timeZone.TypeId != DeclaringUnit.TimeDelta.TypeId)
            {
                ctx.InvalidType(DeclaringUnit.TimeDelta.TypeId, timeZone);
                return false;
            }

            return true;
        }

        private DyDateTime? GetLocalDateTime(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            if (CheckValues(ctx, dateTime, timeZone)) return null;
            var (dt, td) = (((DyDateTime)dateTime).Value, ((DyTimeDelta)timeZone).Value);
            var targetZone = TimeZoneInfo.CreateCustomTimeZone(Guid.NewGuid().ToString(), td, null, null);
            var dat = TimeZoneInfo.ConvertTimeFromUtc(dt, timeZone.NotNil() ? targetZone : TimeZoneInfo.Local);
            return new DyDateTime(DeclaringUnit.DateTime, dat);
        }

        private DyObject ToLocalString(ExecutionContext ctx, DyObject dateTime, DyObject format, DyObject timeZone)
        {
            if (!format.IsString(ctx)) return Default();
            var dt = GetLocalDateTime(ctx, dateTime, timeZone);
            if (dt is null) return Default();
            return DeclaringUnit.DateTime.Format(dt, format, ctx);
        }

        private DyObject GetLocalYear(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            var dt = GetLocalDateTime(ctx, dateTime, timeZone);
            if (dt is null) return Default();
            return new DyInteger(dt.Value.Year);
        }

        private DyObject GetLocalMonth(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            var dt = GetLocalDateTime(ctx, dateTime, timeZone);
            if (dt is null) return Default();
            return new DyInteger(dt.Value.Month);
        }

        private DyObject GetLocalDay(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            var dt = GetLocalDateTime(ctx, dateTime, timeZone);
            if (dt is null) return Default();
            return new DyInteger(dt.Value.Day);
        }

        private DyObject GetLocalTime(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            var dt = GetLocalDateTime(ctx, dateTime, timeZone);
            if (dt is null) return Default();
            return new DyTimeDelta(DeclaringUnit.TimeDelta, dt.Value.TimeOfDay);
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "ToLocalString" => Func.Static(name, ToLocalString, -1, new Par("value"), new Par("format"), new Par("offset", DyNil.Instance)),
                "GetLocalYear" => Func.Static(name, GetLocalYear, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "GetLocalMonth" => Func.Static(name, GetLocalMonth, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "GetLocalDay" => Func.Static(name, GetLocalDay, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "GetLocalTime" => Func.Static(name, GetLocalTime, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "Local" => Func.Auto(name, _ => new DyTimeDelta(DeclaringUnit.TimeDelta, TimeZoneInfo.Local.BaseUtcOffset)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
