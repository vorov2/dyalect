using Dyalect.Codegen;
using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyDateTypeInfo : SpanTypeInfo<DyDate>
{
    private const string Date = nameof(Date);

    public DyDateTypeInfo() : base(Date) { }

    [InstanceMethod("Add")]
    internal static DyObject AddTo(ExecutionContext ctx, DyObject self, int years = 0, int months = 0, int days = 0)
    {
        var s = (DyDate)self.Clone();

        try
        {
            if (days != 0) s.AddDays(days);
            if (months != 0) s.AddMonths(months);
            if (years != 0) s.AddYears(years);
        }
        catch (ArgumentOutOfRangeException)
        {
            return ctx.Overflow();
        }

        return s;
    }

    [InstanceProperty]
    internal static int Year(DyDate self) => self.Year;

    [InstanceProperty]
    internal static int Month(DyDate self) => self.Month;

    [InstanceProperty]
    internal static int Day(DyDate self) => self.Day;

    [InstanceProperty]
    internal static string DayOfWeek(DyDate self) => self.DayOfWeek;

    [InstanceProperty]
    internal static int DayOfYear(DyDate self) => self.DayOfYear;

    [StaticMethod]
    internal static DyObject Parse(ExecutionContext ctx, string input, string format)
    {
        try
        {
            return DyDate.Parse(ctx.Type<DyDateTypeInfo>(), format, input);
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

    [StaticMethod(Date)]
    internal static DyObject CreateNew(ExecutionContext ctx, int year, int month, int day)
    {
        DateTime dt;

        try
        {
            dt = new DateTime(year, month, day).Date;
        }
        catch (Exception)
        {
            return ctx.Overflow();
        }

        return new DyDate(ctx.Type<DyDateTypeInfo>(), (int)(dt.Ticks / DT.TicksPerDay));
    }

    [StaticProperty]
    internal static DyDate Default(ExecutionContext ctx) => Min(ctx);
    
    [StaticProperty]
    internal static DyDate Min(ExecutionContext ctx) => new(ctx.Type<DyDateTypeInfo>(), (int)(DateTime.MinValue.Date.Ticks / DT.TicksPerDay));

    [StaticProperty]
    internal static DyDate Max(ExecutionContext ctx) => new(ctx.Type<DyDateTypeInfo>(), (int)(DateTime.MaxValue.Date.Ticks / DT.TicksPerDay));
}
