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

        private DyObject GetLocalDateTime(ExecutionContext ctx, DyObject dateTime, DyObject timeZone)
        {
            if (CheckValues(ctx, dateTime, timeZone)) return Default();
            var (dt, td) = (((DyDateTime)dateTime).Value, ((DyTimeDelta)timeZone).Value);
            var targetZone = TimeZoneInfo.CreateCustomTimeZone(Guid.NewGuid().ToString(), td, null, null);
            var tzz = timeZone.NotNil() ? targetZone : TimeZoneInfo.Local;
            var dat = TimeZoneInfo.ConvertTimeFromUtc(dt, tzz);
            return new DyLocalDateTime(DeclaringUnit.LocalDateTime, dat, tzz.BaseUtcOffset);
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "ToLocal" => Func.Static(name, GetLocalDateTime, -1, new Par("value"), new Par("offset", DyNil.Instance)),
                "Local" => Func.Auto(name, _ => new DyTimeDelta(DeclaringUnit.TimeDelta, TimeZoneInfo.Local.BaseUtcOffset)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
