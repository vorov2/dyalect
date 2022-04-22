using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core;

public sealed class DyDate : DyForeignObject, IDate, IFormattable
{
    private const string DEFAULT_FORMAT = "yyyy-MM-dd";

    private int days;

    public DyDate(DyDateTypeInfo typeInfo, int days) : base(typeInfo) => this.days = days;

    public long TotalTicks => days * DT.TicksPerDay;

    public int Year => new DateTime(TotalTicks).Year;

    public int Month => new DateTime(TotalTicks).Month;

    public int Day => new DateTime(TotalTicks).Day;

    public string DayOfWeek => new DateTime(TotalTicks).DayOfWeek.ToString();

    public int DayOfYear => new DateTime(TotalTicks).DayOfYear;

    public override object ToObject() => new DateOnly(Year, Month, Day);

    public override DyObject Clone() => new DyDate((DyDateTypeInfo)TypeInfo, days);

    public override int GetHashCode() => days.GetHashCode();

    public override bool Equals(DyObject? other) => other is DyDate dt && dt.days == days;

    public void AddDays(int days) => SetDays(new DateTime(TotalTicks).AddDays(days).Date);

    public void AddMonths(int months) => SetDays(new DateTime(TotalTicks).AddMonths(months).Date);

    public void AddYears(int years) => SetDays(new DateTime(TotalTicks).AddYears(years).Date);

    public static DyDate Parse(DyDateTypeInfo typeInfo, string format, string value)
    {
        var (ticks, _) = InputParser.Parse(FormatParser.DateParser, format, value);
        return new(typeInfo, (int)(ticks / DT.TicksPerDay));
    }

    public string ToString(string? format, IFormatProvider? _ = null)
    {
        var formats = FormatParser.DateParser.ParseSpecifiers(format ?? DEFAULT_FORMAT);
        var sb = new StringBuilder();

        foreach (var f in formats)
            Formatter.FormatDate(this, sb, f);

        return sb.ToString();
    }

    private void SetDays(DateTime dt) => days = (int)(dt.Ticks / DT.TicksPerDay);

    public override string ToString() => ToString(DEFAULT_FORMAT);
}
