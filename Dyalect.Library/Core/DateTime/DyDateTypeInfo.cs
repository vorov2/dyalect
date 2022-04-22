using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core;

public sealed class DyDateTypeInfo : AbstractDateTypeInfo<DyDate>
{
    private const string Date = "Date";

    public DyDateTypeInfo() : base(Date) { }

    private DyObject AddTo(ExecutionContext ctx, DyObject self, DyObject years, DyObject months, DyObject days)
    {
        if (days.NotIntOrNil(ctx)) return Default();
        if (months.NotIntOrNil(ctx)) return Default();
        if (years.NotIntOrNil(ctx)) return Default();
        var s = (DyDate)self.Clone();

        try
        {
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
           "Add" => Func.Member(name, AddTo, -1, new Par("years", 0), new Par("months", 0), new Par("days", 0)),
           _ => base.InitializeInstanceMember(self, name, ctx)
       };

    protected override DyObject Parse(string format, string input) => DyDate.Parse(this, format, input);

    private DyObject New(ExecutionContext ctx, DyObject year, DyObject month, DyObject day)
    {
        if (year.NotNat(ctx)) return Default();
        if (month.NotNat(ctx)) return Default();
        if (day.NotNat(ctx)) return Default();

        if (year.GetInteger() == 0) return ctx.InvalidValue(year);
        if (month.GetInteger() == 0) return ctx.InvalidValue(year);
        if (day.GetInteger() == 0) return ctx.InvalidValue(year);

        DateTime dt;

        try
        {
            dt = new DateTime((int)year.GetInteger(), (int)month.GetInteger(), (int)day.GetInteger()).Date;
        }
        catch (Exception)
        {
            return ctx.Overflow();
        }

        return new DyDate(this, (int)(dt.Ticks / DT.TicksPerDay));
    }

    protected override DyDate GetMin(ExecutionContext ctx) => new(this, (int)(DateTime.MinValue.Date.Ticks / DT.TicksPerDay));

    protected override DyDate GetMax(ExecutionContext ctx) => new(this, (int)(DateTime.MaxValue.Date.Ticks / DT.TicksPerDay));

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
      name switch
      {
          Date => Func.Static(name, New, -1, new Par("year"), new Par("month"), new Par("day")),
          _ => base.InitializeStaticMember(name, ctx)
      };
}
