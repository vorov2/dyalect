using System.Collections.Generic;
using static Dyalect.Library.Core.FormatElementKind;

namespace Dyalect.Library.Core;

internal sealed class FormatParser
{
    private readonly FormatElement[] elements;
    private static readonly FormatElement[] timeDeltaElements = new FormatElement[]
    {
        new(Sign, "+"),
        new(Day, "dd", 2),
        new(Day, "d"),
        new(Hour, "hh", 2),
        new(Hour, "h"),
        new(Minute, "mm", 2),
        new(Minute, "m"),
        new(Second, "ss", 2),
        new(Second, "s"),
        new(Tick, "fffffff", 7),
        new(Microsecond, "ffffff", 6),
        new(HundredthThousandth, "fffff", 5),
        new(TenthThousandth, "ffff", 4),
        new(Millisecond, "fff", 3),
        new(Centisecond, "ff", 2),
        new(Decisecond, "f"),
    };
    private static readonly FormatElement[] timeElements = new FormatElement[]
    {
        new(Hour, "hh", 2),
        new(Hour, "h"),
        new(Minute, "mm", 2),
        new(Minute, "m"),
        new(Second, "ss", 2),
        new(Second, "s"),
        new(Tick, "fffffff", 7),
        new(Microsecond, "ffffff", 6),
        new(HundredthThousandth, "fffff", 5),
        new(TenthThousandth, "ffff", 4),
        new(Millisecond, "fff", 3),
        new(Centisecond, "ff", 2),
        new(Decisecond, "f"),
        new(PmAm, "tt", 2),
        new(PmAm, "t"),
    };
    private static readonly FormatElement[] dateElements = new FormatElement[]
    {
        new(Year,"yyyy", 4),
        new(Year,"yyy", 3),
        new(Year,"yy", 2),
        new(Year,"y", 1),
        new(MonthName,"MMMM"),
        new(MonthAbbrev,"MMM"),
        new(Month,"MM", 2),
        new(Month,"M", 1),
        new(Day, "dd", 2),
        new(Day, "d"),
    };
    private static readonly FormatElement[] dateTimeElements = new FormatElement[]
    {
        new(Year,"yyyy", 4),
        new(Year,"yyy", 3),
        new(Year,"yy", 2),
        new(Year,"y", 1),
        new(MonthName,"MMMM"),
        new(MonthAbbrev,"MMM"),
        new(Month,"MM", 2),
        new(Month,"M", 1),
        new(Day, "dd", 2),
        new(Day, "d"),
        new(Hour, "hh", 2),
        new(Hour, "h"),
        new(Minute, "mm", 2),
        new(Minute, "m"),
        new(Second, "ss", 2),
        new(Second, "s"),
        new(Tick, "fffffff", 7),
        new(Microsecond, "ffffff", 6),
        new(HundredthThousandth, "fffff", 5),
        new(TenthThousandth, "ffff", 4),
        new(Millisecond, "fff", 3),
        new(Centisecond, "ff", 2),
        new(Decisecond, "f"),
        new(PmAm, "tt", 2),
        new(PmAm, "t"),
    };
    private static readonly FormatElement[] localDateElements = new FormatElement[]
    {
        new(Year,"yyyy", 4),
        new(Year,"yyy", 3),
        new(Year,"yy", 2),
        new(Year,"y", 1),
        new(MonthName,"MMMM"),
        new(MonthAbbrev,"MMM"),
        new(Month,"MM", 2),
        new(Month,"M", 1),
        new(Day, "dd", 2),
        new(Day, "d"),
        new(Offset, "zzz", 3),
        new(Offset, "zz", 2),
        new(Offset, "z"),
    };
    private static readonly FormatElement[] localDateTimeElements = new FormatElement[]
    {
        new(Year,"yyyy", 4),
        new(Year,"yyy", 3),
        new(Year,"yy", 2),
        new(Year,"y", 1),
        new(MonthName,"MMMM"),
        new(MonthAbbrev,"MMM"),
        new(Month,"MM", 2),
        new(Month,"M", 1),
        new(Day, "dd", 2),
        new(Day, "d"),
        new(Offset, "zzz", 3),
        new(Offset, "zz", 2),
        new(Offset, "z"),
        new(Hour, "hh", 2),
        new(Hour, "h"),
        new(Minute, "mm", 2),
        new(Minute, "m"),
        new(Second, "ss", 2),
        new(Second, "s"),
        new(Tick, "fffffff", 7),
        new(Microsecond, "ffffff", 6),
        new(HundredthThousandth, "fffff", 5),
        new(TenthThousandth, "ffff", 4),
        new(Millisecond, "fff", 3),
        new(Centisecond, "ff", 2),
        new(Decisecond, "f"),
        new(PmAm, "tt", 2),
        new(PmAm, "t"),
    };
    public static FormatParser TimeDeltaParser { get; } = new FormatParser(timeDeltaElements);
    public static FormatParser TimeParser { get; } = new FormatParser(timeElements);
    public static FormatParser DateParser { get; } = new FormatParser(dateElements);
    public static FormatParser DateTimeParser { get; } = new FormatParser(dateTimeElements);
    public static FormatParser LocalDateParser { get; } = new FormatParser(localDateElements);
    public static FormatParser LocalDateTimeParser { get; } = new FormatParser(localDateTimeElements);

    public FormatParser(FormatElement[] elements) => this.elements = elements;

    public List<FormatElement> ParseSpecifiers(string input)
    {
        var ret = new List<FormatElement>();

        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\')
                i += 1;

            var spc = CheckSpecifiers(input, ref i);

            if (spc is not null)
            {
                ret.Add(spc);
                continue;
            }

            ret.Add(new FormatElement(Literal, input[i].ToString()));
        }

        return ret;
    }

    private FormatElement CheckSpecifiers(string input, ref int i)
    {
        for (var j = 0; j < elements.Length; j++)
            if (CheckSpecifier(input, elements[j].Value, ref i))
                return elements[j];
        return null!;
    }

    private bool CheckSpecifier(string input, string specifier, ref int index)
    {
        var preserved = index;

        if (specifier.Length == 1)
            return input[index] == specifier[0];

        for (var i = 0; i < specifier.Length; i++)
            if (index >= input.Length || specifier[i] != input[index++])
            {
                index = preserved;
                return false;
            }

        index--;
        return true;
    }
}
