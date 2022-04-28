using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyDateTimeTypeInfo : SpanTypeInfo<DyDateTime>
{
    private const string DateTimeType = "DateTime";

    public DyDateTimeTypeInfo() : base(DateTimeType) { }

    protected override SupportedOperations GetSupportedOperations() =>
        base.GetSupportedOperations() | SupportedOperations.Sub | SupportedOperations.Add;

    #region Operations
    protected override DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyDateTime dt)
            try
            {
                return new DyTimeDelta(DeclaringUnit.TimeDelta, ((DyDateTime)left).TotalTicks - dt.TotalTicks);
            }
            catch (Exception)
            {
                return ctx.InvalidValue(right);
            }
        else if (right is DyTimeDelta td)
            try
            {
                return new DyDateTime(this, ((DyDateTime)left).TotalTicks - td.TotalTicks);
            }
            catch (Exception)
            {
                return ctx.InvalidValue(right);
            }

        return ctx.InvalidType(DeclaringUnit.DateTime.TypeId, DeclaringUnit.TimeDelta.TypeId, right);
    }

    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyTimeDelta td)
        {
            try
            {
                return new DyDateTime(this, ((DyDateTime)left).TotalTicks + td.TotalTicks);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue(right);
            }
        }

        return ctx.InvalidType(DeclaringUnit.TimeDelta.TypeId, right);
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType)
    {
        if (targetType.ReflectedTypeId == DeclaringUnit.Date.ReflectedTypeId)
            return ((DyDateTime)self).GetDate(DeclaringUnit.Date);
        else if (targetType.ReflectedTypeId == DeclaringUnit.Time.ReflectedTypeId)
            return ((DyDateTime)self).GetTime(DeclaringUnit.Time);

        return base.CastOp(ctx, self, targetType);
    }
    #endregion

    [InstanceMethod("Add")]
    internal static DyObject AddTo(ExecutionContext ctx, DyObject self, int years = 0, int months = 0, int days = 0,
         double hours = 0, double minutes = 0, double seconds = 0, double milliseconds = 0, long ticks = 0)
    {
        var s = (DyDateTime)self.Clone();

        try
        {
            if (ticks != 0) s.AddTicks(ticks);
            if (milliseconds != 0) s.AddMilliseconds(milliseconds);
            if (seconds != 0) s.AddSeconds(seconds);
            if (minutes != 0) s.AddMinutes(minutes);
            if (hours != 0) s.AddHours(hours);
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
    internal static int Year(DyDateTime self) => self.Year;

    [InstanceProperty]
    internal static int Month(DyDateTime self) => self.Month;

    [InstanceProperty]
    internal static int Day(DyDateTime self) => self.Day;

    [InstanceProperty]
    internal static string DayOfWeek(DyDateTime self) => self.DayOfWeek;

    [InstanceProperty]
    internal static int DayOfYear(DyDateTime self) => self.DayOfYear;

    [InstanceProperty]
    internal static int Hour(DyDateTime self) => self.Hours;

    [InstanceProperty]
    internal static int Minute(DyDateTime self) => self.Minutes;

    [InstanceProperty]
    internal static int Second(DyDateTime self) => self.Seconds;

    [InstanceProperty]
    internal static int Millisecond(DyDateTime self) => self.Milliseconds;

    [InstanceProperty]
    internal static int Tick(DyDateTime self) => self.Ticks;

    [InstanceProperty]
    internal static long TotalTicks(DyDateTime self) => self.TotalTicks;

    [InstanceProperty]
    internal static DyObject Date(ExecutionContext ctx, DyDateTime self) => new DyDate(ctx.Type<DyDateTypeInfo>(), new DateTime(self.TotalTicks));

    [InstanceProperty]
    internal static DyObject Time(ExecutionContext ctx, DyDateTime self) => new DyTime(ctx.Type<DyTimeTypeInfo>(), TimeOnly.FromDateTime(new DateTime(self.TotalTicks)).Ticks);

    [StaticMethod]
    internal static DyObject Parse(ExecutionContext ctx, string input, string format)
    {
        try
        {
            return DyDateTime.Parse(ctx.Type<DyDateTimeTypeInfo>(), format, input);
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

    [StaticMethod(DateTimeType)]
    internal static DyObject CreateNew(ExecutionContext ctx, int year, int month, int day,
        int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
    {
        var dt = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
        return new DyDateTime(ctx.Type<DyDateTimeTypeInfo>(), dt.Ticks);
    }

    [StaticMethod]
    internal static DyObject FromTicks(ExecutionContext ctx, long ticks) =>
        new DyDateTime(ctx.Type<DyDateTimeTypeInfo>(), ticks);

    [StaticMethod]
    internal static DyDateTime Default(ExecutionContext ctx) => Min(ctx);

    [StaticMethod]
    internal static DyDateTime Min(ExecutionContext ctx) => new(ctx.Type<DyDateTimeTypeInfo>(), DateTime.MinValue.Ticks);

    [StaticMethod]
    internal static DyDateTime Max(ExecutionContext ctx) => new(ctx.Type<DyDateTimeTypeInfo>(), DateTime.MaxValue.Ticks);

    [StaticMethod]
    internal static DyDateTime Now(ExecutionContext ctx) => new(ctx.Type<DyDateTimeTypeInfo>(), DateTime.UtcNow.Ticks);
}
