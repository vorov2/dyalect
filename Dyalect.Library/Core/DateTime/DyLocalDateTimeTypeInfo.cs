using Dyalect.Codegen;
using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyLocalDateTimeTypeInfo : SpanTypeInfo<DyDateTime>
{
    private const string LocalDateTime = nameof(LocalDateTime);

    public DyTimeDeltaTypeInfo TypeDeltaTypeInfo => DeclaringUnit.TimeDelta;

    public DyLocalDateTimeTypeInfo() : base(LocalDateTime) { }

    #region Operations
    protected override DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right)
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

    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
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

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType)
    {
        if (targetType.ReflectedTypeId == DeclaringUnit.Date.ReflectedTypeId)
            return ((DyLocalDateTime)self).GetDate(DeclaringUnit.Date);
        else if (targetType.ReflectedTypeId == DeclaringUnit.Time.ReflectedTypeId)
            return ((DyLocalDateTime)self).GetTime(DeclaringUnit.Time);

        return base.CastOp(ctx, self, targetType);
    }
    #endregion

    [InstanceMethod("Add")]
    internal static DyObject AddTo(ExecutionContext ctx, DyObject self, int years = 0, int months = 0, int days = 0,
    double hours = 0, double minutes = 0, double seconds = 0, double milliseconds = 0, long ticks = 0)
    {
        var s = (DyLocalDateTime)self.Clone();

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
    internal static int Year(DyLocalDateTime self) => self.Year;

    [InstanceProperty]
    internal static int Month(DyLocalDateTime self) => self.Month;

    [InstanceProperty]
    internal static int Day(DyLocalDateTime self) => self.Day;

    [InstanceProperty]
    internal static string DayOfWeek(DyLocalDateTime self) => self.DayOfWeek;

    [InstanceProperty]
    internal static int DayOfYear(DyLocalDateTime self) => self.DayOfYear;

    [InstanceProperty]
    internal static int Hour(DyLocalDateTime self) => self.Hours;

    [InstanceProperty]
    internal static int Minute(DyLocalDateTime self) => self.Minutes;

    [InstanceProperty]
    internal static int Second(DyLocalDateTime self) => self.Seconds;

    [InstanceProperty]
    internal static int Millisecond(DyLocalDateTime self) => self.Milliseconds;

    [InstanceProperty]
    internal static int Tick(DyLocalDateTime self) => self.Ticks;

    [InstanceProperty]
    internal static long TotalTicks(DyLocalDateTime self) => self.TotalTicks;

    [InstanceProperty]
    internal static DyObject Date(ExecutionContext ctx, DyDateTime self) => new DyDate(ctx.Type<DyDateTypeInfo>(), new DateTime(self.TotalTicks));

    [InstanceProperty]
    internal static DyObject Time(ExecutionContext ctx, DyDateTime self) => new DyTime(ctx.Type<DyTimeTypeInfo>(), TimeOnly.FromDateTime(new DateTime(self.TotalTicks)).Ticks);

    [InstanceProperty]
    internal static DyObject Offset(DyLocalDateTime self) => self.Offset;

    private static DyTimeDelta GetOffset(ExecutionContext ctx, DyTimeDelta? offset)
    {
        if (offset is null)
            return new DyTimeDelta(ctx.Type<DyTimeDeltaTypeInfo>(), TimeZoneInfo.Local.BaseUtcOffset.Ticks);
        else
            return offset;
    }

    [StaticProperty]
    internal static DyTimeDelta LocalOffset(ExecutionContext ctx) =>
        new(ctx.Type<DyTimeDeltaTypeInfo>(), TimeZoneInfo.Local.BaseUtcOffset);

    [StaticMethod]
    internal static DyObject Parse(ExecutionContext ctx, string input, string format)
    {
        try
        {
            return DyLocalDateTime.Parse(ctx.Type<DyLocalDateTimeTypeInfo>(), format, input);
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

    [StaticMethod(LocalDateTime)]
    internal static DyObject CreateNew(ExecutionContext ctx, int year, int month, int day,
        int hour = 0, int minute = 0, int second = 0, int millisecond = 0, DyTimeDelta? offset = null)
    {
        var delta = GetOffset(ctx, offset);
        var dt = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
        return new DyLocalDateTime(ctx.Type<DyLocalDateTimeTypeInfo>(), dt.Ticks, delta);
    }

    [StaticMethod]
    internal static DyObject FromTicks(ExecutionContext ctx, long ticks, DyTimeDelta? offset = null)
    {
        var delta = GetOffset(ctx, offset);
        return new DyLocalDateTime(ctx.Type<DyLocalDateTimeTypeInfo>(), ticks, delta);
    }

    [StaticMethod]
    internal static DyObject FromDateTime(ExecutionContext ctx, DyDateTime dateTime, DyTimeDelta? offset = null)
    {
        offset = GetOffset(ctx, offset);
        var (dt, td) = (dateTime.ToDateTime(), offset.ToTimeSpan());
        var targetZone = TimeZoneInfo.CreateCustomTimeZone(Guid.NewGuid().ToString(), td, null, null);
        var dat = TimeZoneInfo.ConvertTimeFromUtc(dt, targetZone);
        return new DyLocalDateTime(ctx.Type<DyLocalDateTimeTypeInfo>(), dat.Ticks,
            new DyTimeDelta(ctx.Type<DyTimeDeltaTypeInfo>(), targetZone.BaseUtcOffset));
    }

    [StaticMethod]
    internal static DyLocalDateTime Default(ExecutionContext ctx) => Min(ctx);

    [StaticMethod]
    internal static DyLocalDateTime Min(ExecutionContext ctx) =>
        new(ctx.Type<DyLocalDateTimeTypeInfo>(), DateTime.MinValue.Ticks, GetOffset(ctx, null));

    [StaticMethod]
    internal static DyLocalDateTime Max(ExecutionContext ctx) =>
        new(ctx.Type<DyLocalDateTimeTypeInfo>(), DateTime.MaxValue.Ticks, GetOffset(ctx, null));

    [StaticMethod]
    internal static DyLocalDateTime Now(ExecutionContext ctx) =>
        new(ctx.Type<DyLocalDateTimeTypeInfo>(), DateTime.Now.Ticks, GetOffset(ctx, null));
}
