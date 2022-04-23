using System;
using System.Text;
using static Dyalect.Library.Core.FormatElementKind;
namespace Dyalect.Library.Core;

internal static class Formatter
{
    public static string Format(long val, FormatElement fe)
    {
        val = Math.Abs(val);

        if (fe.Padding == 0 && val == 0)
            return string.Empty;

        return fe.Padding > 1 ? val.ToString().PadLeft(fe.Padding, '0') : val.ToString();
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

    public static bool FormatLocalDateTime(this ILocalDateTime self, StringBuilder builder, FormatElement elem)
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
                        builder.Append(Math.Abs(self.Interval.Hours));
                        return true;
                    }
                    else if (elem.Padding == 2)
                    {
                        builder.Append(Math.Abs(self.Interval.Hours).ToString().PadLeft(2, '0'));
                        return true;
                    }
                    else if (elem.Padding == 3)
                    {
                        builder.Append(Math.Abs(self.Interval.Hours).ToString().PadLeft(2, '0'));
                        builder.Append(CI.UI.DateTimeFormat.TimeSeparator);
                        builder.Append(Math.Abs(self.Interval.Minutes).ToString().PadLeft(2, '0'));
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
            case Literal:
                builder.Append(elem.Value);
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
}
