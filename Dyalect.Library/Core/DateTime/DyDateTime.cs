using Dyalect.Runtime.Types;
using System;
using System.Text;
namespace Dyalect.Library.Core;

public class DyDateTime : DyForeignObject, IDateTime, IFormattable
{
    private const string FORMAT = "yyyy-MM-dd HH:mm:ss.fffffff";

    protected long ticks;

    internal DyDateTime(SpanTypeInfo<DyDateTime> typeInfo, long ticks) : base(typeInfo) =>
        this.ticks = ticks;

    public DateTime ToDateTime() => new(ticks);

    public override object ToObject() => ToDateTime();

    public override DyObject Clone() => new DyDateTime((SpanTypeInfo<DyDateTime>)TypeInfo, ticks);

    public override bool Equals(DyObject? other) => other is DyDateTime dt && dt.ticks == ticks;

    public override int GetHashCode() => ticks.GetHashCode();

    public override string ToString() => ToString(FORMAT);

    public long ToInteger() => ticks;

    public static DyDateTime Parse(DyForeignTypeInfo typeInfo, string format, string value)
    {
        var (ticks, _) = InputParser.Parse(FormatParser.DateTimeParser, format, value);
        return new((SpanTypeInfo<DyDateTime>)typeInfo, ticks);
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
        return new DyDateTime((SpanTypeInfo<DyDateTime>)TypeInfo, dt.AddDays(-dt.Day + 1).Ticks);
    }

    public virtual DyDateTime LastDayOfMonth()
    {
        var dt = new DateTime(ticks, DateTimeKind.Unspecified);
        return new DyDateTime((SpanTypeInfo<DyDateTime>)TypeInfo, dt.AddDays(DateTime.DaysInMonth(dt.Year, dt.Month) - dt.Day).Ticks);
    }

    #region DateTime
    public long TotalTicks => ticks;

    public int Ticks => (int)(ticks % 10_000_000);

    public int Microseconds => (int)(ticks / DT.TicksPerMicrosecond % 1_000_000);

    public int Milliseconds => (int)(ticks / DT.TicksPerMillisecond % 1000);

    public int Seconds => (int)(ticks / DT.TicksPerSecond % 60);

    public int Minutes => (int)(ticks / DT.TicksPerMinute % 60);

    public int Hours => (int)(ticks / DT.TicksPerHour % 24);

    public int Year => new DateTime(ticks).Year;

    public int Month => new DateTime(ticks).Month;

    public int Day => new DateTime(ticks).Day;

    public string DayOfWeek => new DateTime(ticks).DayOfWeek.ToString();

    public int DayOfYear => new DateTime(ticks).DayOfYear;

    public void AddDays(int value) => SetTicks(new DateTime(ticks).AddDays(value));

    public void AddMonths(int value) => SetTicks(new DateTime(ticks).AddMonths(value));

    public void AddYears(int value) => SetTicks(new DateTime(ticks).AddYears(value));

    public void AddHours(double value) => SetTicks(new DateTime(ticks).AddHours(value));

    public void AddMinutes(double value) => SetTicks(new DateTime(ticks).AddMinutes(value));

    public void AddSeconds(double value) => SetTicks(new DateTime(ticks).AddSeconds(value));

    public void AddMilliseconds(double value) => SetTicks(new DateTime(ticks).AddMilliseconds(value));

    public void AddTicks(long value) => ticks += value;

    public DyObject GetDate(DyDateTypeInfo typeInfo) =>
        new DyDate(typeInfo, DateOnly.FromDateTime(new DateTime(ticks)).DayNumber);

    public DyObject GetTime(DyTimeTypeInfo typeInfo) =>
        new DyTime(typeInfo, TimeOnly.FromDateTime(new DateTime(ticks)).Ticks);

    private void SetTicks(DateTime dt) => ticks = dt.Ticks;
    #endregion
}

