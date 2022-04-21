using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public abstract class DyBaseDateTimeTypeInfo : DyForeignTypeInfo<CoreModule>
    {
        public override string TypeName { get; }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Sub | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Gte
            | SupportedOperations.Lt | SupportedOperations.Lte;

        protected DyBaseDateTimeTypeInfo(string typeName) => TypeName = typeName;

        protected abstract DyDateTime CreateDateTime(DateTime dateTime, TimeSpan? offset);

        internal DyObject Format(DyDateTime dateTime, DyObject format, ExecutionContext ctx) =>
            ToStringOp(dateTime, format, ctx);

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).GetTicks() == ((DyDateTime)right).GetTicks() ? DyBool.True : DyBool.False;
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).GetTicks() != ((DyDateTime)right).GetTicks() ? DyBool.True : DyBool.False;
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).GetTicks() > ((DyDateTime)right).GetTicks() ? DyBool.True : DyBool.False;
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).GetTicks() >= ((DyDateTime)right).GetTicks() ? DyBool.True : DyBool.False;
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).GetTicks() < ((DyDateTime)right).GetTicks() ? DyBool.True : DyBool.False;
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).GetTicks() <= ((DyDateTime)right).GetTicks() ? DyBool.True : DyBool.False;
        }

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => new DyInteger(((DyDateTime)self).GetTicks()),
                _ => base.CastOp(self, targetType, ctx)
            };

        private DyObject AddTo(ExecutionContext ctx, DyObject self, DyObject ticks, DyObject ms, DyObject sec,
            DyObject min, DyObject hrs, DyObject days, DyObject months, DyObject years)
        {
            var dateTime = (DyDateTime)self;
            var dt = dateTime.Value;

            if (!ticks.NotNil() && !ticks.IsInteger(ctx)) return Default();
            if (!ms.NotNil() && !ms.IsInteger(ctx)) return Default();
            if (!sec.NotNil() && !sec.IsInteger(ctx)) return Default();
            if (!min.NotNil() && !min.IsInteger(ctx)) return Default();
            if (!hrs.NotNil() && !hrs.IsInteger(ctx)) return Default();
            if (!days.NotNil() && !days.IsInteger(ctx)) return Default();
            if (!months.NotNil() && !months.IsInteger(ctx)) return Default();
            if (!years.NotNil() && !years.IsInteger(ctx)) return Default();

            try
            {
                if (ticks.NotNil()) dt = dt.AddTicks(ticks.GetInteger());
                if (ms.NotNil()) dt = dt.AddMilliseconds(ms.GetFloat());
                if (sec.NotNil()) dt = dt.AddSeconds(sec.GetFloat());
                if (min.NotNil()) dt = dt.AddMinutes(min.GetFloat());
                if (hrs.NotNil()) dt = dt.AddHours(hrs.GetFloat());
                if (days.NotNil()) dt = dt.AddDays(days.GetFloat());
                if (months.NotNil()) dt = dt.AddMonths((int)months.GetInteger());
                if (years.NotNil()) dt = dt.AddYears((int)years.GetInteger());
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.Overflow();
            }

            return CreateDateTime(dt, dateTime.Offset);
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Add" => Func.Member(name, AddTo, -1, new Par("ticks", DyInteger.Zero), new Par("milliseconds", DyInteger.Zero), new Par("seconds", DyInteger.Zero),
                    new Par("minutes", DyInteger.Zero), new Par("hours", DyInteger.Zero), new Par("days", DyInteger.Zero), new Par("months", DyInteger.Zero),
                    new Par("years", DyInteger.Zero)),
                "Ticks" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Ticks)),
                "Millisecond" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Millisecond)),
                "Second" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Second)),
                "Minute" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Minute)),
                "Hour" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Hour)),
                "Day" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Day)),
                "Month" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Month)),
                "Year" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Year)),
                "DayOfWeek" => Func.Auto(name, _ => new DyString(((DyDateTime)self).Value.DayOfWeek.ToString())),
                "Date" => Func.Auto(name, _ => CreateDateTime(((DyDateTime)self).Value.Date, ((DyDateTime)self).Offset)),
                "Time" => Func.Auto(name, _ => new DyTimeDelta(DeclaringUnit.TimeDelta, ((DyDateTime)self).Value.TimeOfDay)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        protected DyObject New(ExecutionContext ctx, DyObject year, DyObject month, DyObject day, DyObject hour, DyObject minute, DyObject second, DyObject millisecond, TimeSpan? offset = null)
        {
            if (!year.IsInteger(ctx)) return Default();
            if (!month.IsInteger(ctx)) return Default();
            if (!day.IsInteger(ctx)) return Default();
            if (!hour.IsInteger(ctx)) return Default();
            if (!minute.IsInteger(ctx)) return Default();
            if (!second.IsInteger(ctx)) return Default();
            if (!millisecond.IsInteger(ctx)) return Default();

            try
            {
                var dt = new DateTime((int)year.GetInteger(), (int)month.GetInteger(), (int)day.GetInteger(), (int)hour.GetInteger(),
                    (int)minute.GetInteger(), (int)second.GetInteger(), (int)millisecond.GetInteger(),
                    offset is null ? DateTimeKind.Utc : DateTimeKind.Local);
                return CreateDateTime(dt, offset);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue();
            }
        }

        protected DyObject FromTicks(ExecutionContext ctx, DyObject ticks, TimeSpan? offset)
        {
            if (!ticks.IsInteger(ctx)) return Default();

            try
            {
                var dt = new DateTime(ticks.GetInteger(), offset is null ? DateTimeKind.Utc : DateTimeKind.Local);
                return CreateDateTime(dt, offset);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue();
            }
        }
    }
}
