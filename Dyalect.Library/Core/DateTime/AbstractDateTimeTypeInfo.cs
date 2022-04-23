using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Library.Core;

public abstract class AbstractDateTimeTypeInfo<T> : AbstractDateTypeInfo<T>
    where T : DyObject, IDateTime, IFormattable
{
    protected AbstractDateTimeTypeInfo(string typeName) : base(typeName) { }

    protected override long ToInteger(DyObject self) => ((T)self).TotalTicks;

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx)
    {
        if (targetType.ReflectedTypeId == DeclaringUnit.Date.ReflectedTypeId)
            return ((T)self).GetDate(DeclaringUnit.Date);
        else if (targetType.ReflectedTypeId == DeclaringUnit.Time.ReflectedTypeId)
            return ((T)self).GetTime(DeclaringUnit.Time);

        return base.CastOp(self, targetType, ctx);
    }

    private DyObject AddTo(ExecutionContext ctx, DyObject self, DyObject years, DyObject months, DyObject days,
         DyObject hrs, DyObject min, DyObject sec, DyObject ms, DyObject ticks)
    {
        if (ticks.NotIntOrNil(ctx)) return Default();
        if (ms.NotIntOrNil(ctx)) return Default();
        if (sec.NotIntOrNil(ctx)) return Default();
        if (min.NotIntOrNil(ctx)) return Default();
        if (hrs.NotIntOrNil(ctx)) return Default();
        if (days.NotIntOrNil(ctx)) return Default();
        if (months.NotIntOrNil(ctx)) return Default();
        if (years.NotIntOrNil(ctx)) return Default();
        var s = (T)self.Clone();

        try
        {
            if (ticks.NotNil()) s.AddTicks(ticks.GetInteger());
            if (ms.NotNil()) s.AddMilliseconds(ms.GetFloat());
            if (sec.NotNil()) s.AddSeconds(sec.GetFloat());
            if (min.NotNil()) s.AddMinutes(min.GetFloat());
            if (hrs.NotNil()) s.AddHours(hrs.GetFloat());
            if (days.NotNil()) s.AddDays((int)days.GetInteger());
            if (months.NotNil()) s.AddMonths((int)months.GetInteger());
            if (years.NotNil()) s.AddYears((int)years.GetInteger());
        }
        catch (ArgumentOutOfRangeException)
        {
            return ctx.Overflow();
        }

        return s;
    }

    protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
       name switch
       {
           "Add" => Func.Member(name, AddTo, -1, new Par("years", 0), new Par("months", 0), new Par("days", 0),
               new Par("hours", 0), new Par("minutes", 0), new Par("seconds", 0), new Par("milliseconds", 0), new Par("ticks", 0)),
           "Hour" => Func.Auto(name, (_, s) => new DyInteger(((T)s).Hours)),
           "Minute" => Func.Auto(name, (_, s) => new DyInteger(((T)s).Minutes)),
           "Second" => Func.Auto(name, (_, s) => new DyInteger(((T)s).Seconds)),
           "Millisecond" => Func.Auto(name, (_, s) => new DyInteger(((T)s).Milliseconds)),
           "Tick" => Func.Auto(name, (_, s) => new DyInteger(((T)s).Ticks)),
           "Date" => Func.Auto(name, (_, s) => new DyDate(DeclaringUnit.Date, new DateTime(((T)s).TotalTicks))),
           "Time" => Func.Auto(name, (_, s) => new DyTime(DeclaringUnit.Time, TimeOnly.FromDateTime(new DateTime(((T)s).TotalTicks)).Ticks)),
           _ => base.InitializeInstanceMember(self, name, ctx)
       };

    protected abstract DyObject Create(long ticks, DyTimeDelta? offset);

    protected DyObject New(ExecutionContext ctx, DyObject year, DyObject month, DyObject day, DyObject hour, DyObject minute, DyObject second, DyObject millisecond, DyObject? offset = null)
    {
        if (year.NotInteger(ctx)) return Default();
        if (month.NotInteger(ctx)) return Default();
        if (day.NotInteger(ctx)) return Default();
        if (hour.NotInteger(ctx)) return Default();
        if (minute.NotInteger(ctx)) return Default();
        if (second.NotInteger(ctx)) return Default();
        if (millisecond.NotInteger(ctx)) return Default();

        if (offset is not null && offset is not DyTimeDelta)
            return ctx.InvalidType(DeclaringUnit.TimeDelta.ReflectedTypeId, offset);

        try
        {
            var dt = new DateTime((int)year.GetInteger(), (int)month.GetInteger(), (int)day.GetInteger(), (int)hour.GetInteger(),
                (int)minute.GetInteger(), (int)second.GetInteger(), (int)millisecond.GetInteger(),
                offset is null ? DateTimeKind.Utc : DateTimeKind.Local);
            return Create(dt.Ticks, offset as DyTimeDelta);
        }
        catch (ArgumentOutOfRangeException)
        {
            return ctx.InvalidValue();
        }
    }
}
