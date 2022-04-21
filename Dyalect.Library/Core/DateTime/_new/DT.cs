using System;
using System.Text;
using static Dyalect.Library.Core.FormatElementKind;

namespace Dyalect.Library.Core;

public interface ITime
{
    int Hours { get; }
    int Minutes { get; }
    int Seconds { get; }
    int Milliseconds { get; }
    int Microseconds { get; }
    int Ticks { get; }
}

public interface IDate
{
    int Year { get; }
    int Month { get; }  
    int Day { get; }
}

public interface IDateTime : IDate, ITime
{

}

public interface IZonedDateTime : IDateTime
{
    IInterval Interval { get; }
}

public interface IInterval : ITime
{
    int Days { get; }

    long TotalTicks { get; }
}

internal static class DT
{
    public const long TicksPerDay = 24 * TicksPerHour;
    public const long TicksPerHour = 60 * TicksPerMinute;
    public const long TicksPerMinute = 60 * TicksPerSecond;
    public const long TicksPerSecond = 10 * TicksPerDecisecond;
    public const long TicksPerDecisecond = 10 * TicksPerCentisecond;
    public const long TicksPerCentisecond = 10 * TicksPerMillisecond;
    public const long TicksPerMillisecond = 1000 * TicksPerMicrosecond;
    public const long TicksPerMicrosecond = 10L;

    public static string Format(long val, FormatElement fe)
    {
        val = Math.Abs(val);

        if (fe.Padding == 0 && val == 0)
            return string.Empty;

        return fe.Padding > 1 ? val.ToString().PadLeft(2, '0') : val.ToString();
    }

    public static bool FormatInterval(this IInterval self, StringBuilder builder, FormatElement elem)
    {
        switch (elem.Kind)
        {
            case Sign:
                if (self.TotalTicks > 0)
                    builder.Append('+');
                else if (self.TotalTicks < 0)
                    builder.Append('-');
                return true;
            case Day:
                builder.Append(Format(self.Days, elem));
                return true;
            default:
                return FormatTime(self, builder, elem);
        }
    }

    public static bool FormatZonedDateTime(this IZonedDateTime self, StringBuilder builder, FormatElement elem)
    {
        if (!FormatDate(self, builder, elem))
            if (!FormatTime(self, builder, elem))
            {
                if (elem.Kind == Offset)
                {
                    if (self.Interval.TotalTicks == 0)
                        return true;
                    else if (self.Interval.TotalTicks < 0)
                        builder.Append('-');
                    else
                        builder.Append('+');

                    if (elem.Padding == 1)
                    {
                        builder.Append(self.Interval.Hours);
                        return true;
                    }
                    else if (elem.Padding == 2)
                    {
                        builder.Append(self.Interval.Hours.ToString().PadLeft(2, '0'));
                        return true;
                    }
                    else if (elem.Padding == 3)
                    {
                        builder.Append(self.Interval.Hours.ToString().PadLeft(2, '0'));
                        builder.Append(CI.UI.DateTimeFormat.TimeSeparator);
                        builder.Append(self.Interval.Minutes.ToString().PadLeft(2, '0'));
                        return true;
                    }
                }
            }

        return false;
    }

    public static bool FormatDateTime(this IDateTime self, StringBuilder builder, FormatElement elem)
    {
        if (!FormatDate(self, builder, elem))
            return FormatTime(self, builder, elem);

        return true;
    }

    public static bool FormatDate(this IDate self, StringBuilder builder, FormatElement elem)
    {
        switch (elem.Kind)
        {
            case Year:
                if (elem.Padding == 1)
                    builder.Append(self.Year % 100);
                else if (elem.Padding == 2)
                    builder.Append((self.Year % 100).ToString().PadLeft(2, '0'));
                else if (elem.Padding == 3)
                    builder.Append(self.Year.ToString().PadLeft(3, '0'));
                else if (elem.Padding == 4)
                    builder.Append(self.Year.ToString().PadLeft(4, '0'));
                return true;
            case MonthAbbrev:
                builder.Append(new DateTime(self.Year, self.Month, self.Day).ToString("MMM", CI.UI));
                return true;
            case MonthName:
                builder.Append(new DateTime(self.Year, self.Month, self.Day).ToString("MMMM", CI.UI));
                return true;
            case Month:
                builder.Append(Format(self.Month, elem));
                return true;
            case Day:
                builder.Append(Format(self.Day, elem));
                return true;
            default:
                return false;
        }
    }

    public static bool FormatTime(this ITime self, StringBuilder builder, FormatElement elem)
    {
        switch (elem.Kind)
        {
            case Hour:
                builder.Append(Format(self.Hours, elem));
                return true;
            case Minute:
                builder.Append(Format(self.Minutes, elem));
                return true;
            case Second:
                builder.Append(Format(self.Seconds, elem));
                return true;
            case Decisecond:
                builder.Append(Format(self.Milliseconds / 100, elem));
                return true;
            case Centisecond:
                builder.Append(Format(self.Milliseconds / 10, elem));
                return true;
            case Millisecond:
                builder.Append(Format(self.Milliseconds, elem));
                return true;
            case TenthThousandth:
                builder.Append(Format(self.Microseconds / 100, elem));
                return true;
            case HundredthThousandth:
                builder.Append(Format(self.Microseconds / 10, elem));
                return true;
            case Microsecond:
                builder.Append(Format(self.Microseconds, elem));
                return true;
            case Tick:
                builder.Append(Format(self.Ticks, elem));
                return true;
            case PmAm:
                {
                    var dt = new DateTime(1, 1, 1, self.Hours, self.Minutes, self.Seconds);
                    if (elem.Padding == 1)
                        builder.Append(dt.ToString("t", CI.UI));
                    else if (elem.Padding == 2)
                        builder.Append(dt.ToString("tt", CI.UI));
                    return true;
                }
            case Literal:
                builder.Append(elem.Value);
                return true;
            default:
                return false;
        }
    }

    public static long Parse(FormatParser formatParser, string format, string value)
    {
        var formats = formatParser.ParseSpecifiers(format);
        var chunks = InputParser.Parse(formats, value);
        var (days, hours, minutes, seconds, ds, cs, ms, tts, hts, micros, tick) =
            (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        var negate = false;

        foreach (var (kind, val) in chunks)
            switch (kind)
            {
                case Sign:
                    negate = val == "-";
                    break;
                case Day:
                    if (!int.TryParse(val, out days)) throw new FormatException();
                    break;
                case Hour:
                    if (!int.TryParse(val, out hours)) throw new FormatException();
                    if (hours is < 0 or > 23) throw new OverflowException();
                    break;
                case Minute:
                    if (!int.TryParse(val, out minutes)) throw new FormatException();
                    if (minutes is < 0 or > 59) throw new OverflowException();
                    break;
                case Second:
                    if (!int.TryParse(val, out seconds)) throw new FormatException();
                    if (seconds is < 0 or > 59) throw new OverflowException();
                    break;
                case Decisecond:
                    if (!int.TryParse(val, out ds)) throw new FormatException();
                    if (ds is < 0 or > 9) throw new OverflowException();
                    break;
                case Centisecond:
                    if (!int.TryParse(val, out cs)) throw new FormatException();
                    if (cs is < 0 or > 99) throw new OverflowException();
                    break;
                case Millisecond:
                    if (!int.TryParse(val, out ms)) throw new FormatException();
                    if (ms is < 0 or > 999) throw new OverflowException();
                    break;
                case TenthThousandth:
                    if (!int.TryParse(val, out tts)) throw new FormatException();
                    if (tts is < 0 or > 9_999) throw new OverflowException();
                    break;
                case HundredthThousandth:
                    if (!int.TryParse(val, out hts)) throw new FormatException();
                    if (hts is < 0 or > 99_999) throw new OverflowException();
                    break;
                case Microsecond:
                    if (!int.TryParse(val, out micros)) throw new FormatException();
                    if (micros is < 0 or > 999_999) throw new OverflowException();
                    break;
                case Tick:
                    if (!int.TryParse(val, out tick)) throw new FormatException();
                    if (tick is < 0 or > 9_999_999) throw new OverflowException();
                    break;
            }

        var totalTicks =
            tick +
            micros * DT.TicksPerMicrosecond +
            hts * DT.TicksPerMillisecond * 100 +
            tts * DT.TicksPerMillisecond * 10 +
            ms * DT.TicksPerMillisecond +
            cs * DT.TicksPerCentisecond +
            ds * DT.TicksPerDecisecond +
            seconds * DT.TicksPerSecond +
            minutes * DT.TicksPerMinute +
            hours * DT.TicksPerHour +
            days * DT.TicksPerDecisecond;

        if (negate)
            totalTicks = -totalTicks;

        return totalTicks;
    }
}
