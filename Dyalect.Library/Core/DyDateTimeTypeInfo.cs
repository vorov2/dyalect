using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyDateTimeTypeInfo : DyForeignTypeInfo<CoreModule>
    {
        private const string FORMAT = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz";
        public override string TypeName => "DateTime";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Sub | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Gte
            | SupportedOperations.Lt | SupportedOperations.Lte;

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
        {
            if (format.TypeId == DyType.Nil)
                return new DyString(((DyDateTime)arg).Value.ToString(FORMAT, CI.Default));

            try
            {
                var res = ((DyDateTime)arg).Value.ToString(format.GetString(), CI.Default);
                return new DyString(res);
            }
            catch (Exception)
            {
                return ctx.InvalidValue(format);
            }
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right is DyDateTime dt)
                try
                {
                    return new DyTimeDelta(DeclaringUnit.TimeDelta, ((DyDateTime)left).Value - dt.Value);
                }
                catch (Exception)
                {
                    return ctx.InvalidValue(right);
                }
            else if (right is DyTimeDelta td)
                try
                {
                    return new DyDateTime(this, ((DyDateTime)left).Value - td.Value);
                }
                catch (Exception)
                {
                    return ctx.InvalidValue(right);
                }

            return ctx.InvalidType(DeclaringUnit.DateTime.TypeId, DeclaringUnit.TimeDelta.TypeId, right);
        }

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right is DyTimeDelta td)
            {
                try
                {
                    return new DyDateTime(this, ((DyDateTime)left).Value + td.Value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return ctx.InvalidValue(right);
                }
            }

            return ctx.InvalidType(DeclaringUnit.TimeDelta.TypeId, right);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).Value > ((DyDateTime)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).Value >= ((DyDateTime)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).Value < ((DyDateTime)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyDateTime)left).Value <= ((DyDateTime)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => new DyInteger(((DyDateTime)self).Value.Ticks),
                _ => base.CastOp(self, targetType, ctx)
            };

        private DyObject AddTo(ExecutionContext ctx, DyObject self, DyObject ticks, DyObject ms, DyObject sec, 
            DyObject min, DyObject hrs, DyObject days, DyObject months, DyObject years)
        {
            var dt = ((DyDateTime)self).Value;

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
                if (!ticks.NotNil()) dt = dt.AddTicks(ticks.GetInteger());
                if (!ms.NotNil()) dt = dt.AddMilliseconds(ms.GetFloat());
                if (!sec.NotNil()) dt = dt.AddSeconds(sec.GetFloat());
                if (!min.NotNil()) dt = dt.AddMinutes(min.GetFloat());
                if (!hrs.NotNil()) dt = dt.AddHours(hrs.GetFloat());
                if (!days.NotNil()) dt = dt.AddDays(days.GetFloat());
                if (!months.NotNil()) dt = dt.AddMonths((int)months.GetInteger());
                if (!years.NotNil()) dt = dt.AddYears((int)years.GetInteger());
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.Overflow();
            }

            return new DyDateTime(this, dt);
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Add" => Func.Member(name, AddTo, -1, new Par("ticks", DyNil.Instance), new Par("milliseconds", DyNil.Instance), new Par("seconds", DyNil.Instance),
                    new Par("minutes", DyNil.Instance), new Par("hours", DyNil.Instance), new Par("days", DyNil.Instance), new Par("months", DyNil.Instance),
                    new Par("years", DyNil.Instance)),
                "Ticks" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Ticks)),
                "Millisecond" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Millisecond)),
                "Second" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Second)),
                "Minute" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Minute)),
                "Hour" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Hour)),
                "Day" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Day)),
                "Month" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Month)),
                "Year" => Func.Auto(name, _ => DyInteger.Get(((DyDateTime)self).Value.Year)),
                "DayOfWeek" => Func.Auto(name, _ => new DyString(((DyDateTime)self).Value.DayOfWeek.ToString())),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Parse(ExecutionContext ctx, DyObject value, DyObject format)
        {
            if (!value.IsString(ctx)) return Default();
            if (format.NotNil() && !format.IsString(ctx)) return Default();

            try
            {
                return format.NotNil() 
                    ? new DyDateTime(this, DateTime.ParseExact(value.GetString(), format.GetString(), CI.Default))
                    : new DyDateTime(this, DateTime.Parse(value.GetString(), CI.Default));
            }
            catch (FormatException ex)
            {
                return ctx.ParsingFailed(ex.Message);
            }
        }

        private DyObject New(ExecutionContext ctx, DyObject year, DyObject month, DyObject day, DyObject hour, DyObject minute, DyObject second, DyObject millisecond)
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
                    (int)minute.GetInteger(), (int)second.GetInteger(), (int)millisecond.GetInteger(), DateTimeKind.Local);
                return new DyDateTime(this, dt);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue();
            }
        }

        private DyObject FromTicks(ExecutionContext ctx, DyObject ticks)
        {
            if (!ticks.IsInteger(ctx)) return Default();

            try
            {
                var dt = new DateTime((int)ticks.GetInteger(), DateTimeKind.Local);
                return new DyDateTime(this, dt);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue();
            }
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch 
            {
                "Now" => Func.Static(name, _ => new DyDateTime(this, DateTime.Now)),
                "Parse" => Func.Static(name, Parse, -1, new Par("value"), new Par("format", DyNil.Instance)),
                "DateTime" => Func.Static(name, New, -1, new Par("year", DyInteger.Zero), new Par("month", DyInteger.Zero), 
                    new Par("day", DyInteger.Zero), new Par("hour", DyInteger.Zero), new Par("minute", DyInteger.Zero),
                    new Par("second", DyInteger.Zero), new Par("millisecond", DyInteger.Zero)),
                "FromTicks" => Func.Static(name, FromTicks, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
