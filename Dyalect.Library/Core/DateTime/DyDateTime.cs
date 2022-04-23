using Dyalect.Runtime.Types;
using System;
using System.Text;
namespace Dyalect.Library.Core;

public class DyDateTime : DyForeignObject, IDateTime, IFormattable
{
    private const string FORMAT = "yyyy-MM-dd hh:mm:ss.fffffff";

    protected long ticks;

    public long Ticks => ticks;

    internal DyDateTime(AbstractDateTimeTypeInfo<DyDateTime> typeInfo, long ticks) : base(typeInfo) =>
        this.ticks = ticks;

    public DateTime ToDateTime() => new(ticks);

    public override object ToObject() => ToDateTime();

    public override DyObject Clone() => new DyDateTime((AbstractDateTimeTypeInfo<DyDateTime>)TypeInfo, ticks);

    public override bool Equals(DyObject? other) => other is DyDateTime dt && dt.ticks == ticks;

    public override int GetHashCode() => ticks.GetHashCode();

    public override string ToString() => ToString(FORMAT);

    public static DyDateTime Parse(DyForeignTypeInfo typeInfo, string format, string value)
    {
        var (ticks, _) = InputParser.Parse(FormatParser.DateTimeParser, format, value);
        return new((AbstractDateTimeTypeInfo<DyDateTime>)typeInfo, ticks);
    }

    public virtual string ToString(string? format, IFormatProvider? _ = null)
    {
        var formats = FormatParser.DateTimeParser.ParseSpecifiers(format ?? FORMAT);
        var sb = new StringBuilder();

        foreach (var f in formats)
            Formatter.FormatDateTime(this, sb, f);

        return sb.ToString();
    }

    public virtual DyDateTime FirstDayOfMonth()
    {
        var dt = new DateTime(ticks, DateTimeKind.Unspecified);
        return new DyDateTime((AbstractDateTimeTypeInfo<DyDateTime>)TypeInfo, dt.AddDays(-dt.Day + 1).Ticks);
    }

    public virtual DyDateTime LastDayOfMonth()
    {
        var dt = new DateTime(ticks, DateTimeKind.Unspecified);
        return new DyDateTime((AbstractDateTimeTypeInfo<DyDateTime>)TypeInfo, dt.AddDays(DateTime.DaysInMonth(dt.Year, dt.Month) - dt.Day).Ticks);
    }

    #region DateTime
    long ISpan.TotalTicks => ticks;

    int ITime.Ticks => (int)(ticks % 10_000_000);

    int ITime.Microseconds => (int)(ticks / DT.TicksPerMicrosecond % 1_000_000);

    int ITime.Milliseconds => (int)(ticks / DT.TicksPerMillisecond % 1000);

    int ITime.Seconds => (int)(ticks / DT.TicksPerSecond % 60);

    int ITime.Minutes => (int)(ticks / DT.TicksPerMinute % 60);

    int ITime.Hours => (int)(ticks / DT.TicksPerHour % 24);

    int IDate.Year => new DateTime(ticks).Year;

    int IDate.Month => new DateTime(ticks).Month;

    int IDate.Day => new DateTime(ticks).Day;

    string IDate.DayOfWeek => new DateTime(ticks).DayOfWeek.ToString();

    int IDate.DayOfYear => new DateTime(ticks).DayOfYear;

    void IDate.AddDays(int value) => SetTicks(new DateTime(ticks).AddDays(value));

    void IDate.AddMonths(int value) => SetTicks(new DateTime(ticks).AddMonths(value));

    void IDate.AddYears(int value) => SetTicks(new DateTime(ticks).AddYears(value));

    void IDateTime.AddHours(double value) => SetTicks(new DateTime(ticks).AddHours(value));

    void IDateTime.AddMinutes(double value) => SetTicks(new DateTime(ticks).AddMinutes(value));

    void IDateTime.AddSeconds(double value) => SetTicks(new DateTime(ticks).AddSeconds(value));

    void IDateTime.AddMilliseconds(double value) => SetTicks(new DateTime(ticks).AddMilliseconds(value));

    void IDateTime.AddTicks(long value) => ticks += value;

    DyObject IDateTime.GetDate(DyDateTypeInfo typeInfo) =>
        new DyDate(typeInfo, DateOnly.FromDateTime(new DateTime(ticks)).DayNumber);

    DyObject IDateTime.GetTime(DyTimeTypeInfo typeInfo) =>
        new DyTime(typeInfo, TimeOnly.FromDateTime(new DateTime(ticks)).Ticks);

    private void SetTicks(DateTime dt) => ticks = dt.Ticks;
    #endregion
}

