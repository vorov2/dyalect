using System.Collections.Generic;
using static Dyalect.Library.Core.FormatElementKind;

namespace Dyalect.Library.Core
{
    internal enum FormatElementKind
    {
        Literal,
        Plus,
        Day,
        Hour,
        Minute,
        Second,
        Decisecond,
        Centisecond,
        Millisecond,
        Microsecond,
        Tick
    }

    internal record FormatElement(FormatElementKind Kind, string Value, int Padding = 0, bool DisplayIfZero = true);

    internal sealed class FormatStringParser
    {
        private readonly FormatElement[] elements;
        private static readonly FormatElement[] timeDeltaElements = new FormatElement[]
        {
            new(Plus, "+"),
            new(Day, "dd", 2),
            new(Day, "d"),
            new(Hour, "hh", 2),
            new(Hour, "h"),
            new(Minute, "mm", 2),
            new(Minute, "m"),
            new(Second, "ss", 2),
            new(Second, "s"),
            new(Tick, "fffff", 7),
            new(Microsecond, "ffff", 6),
            new(Millisecond, "fff", 3),
            new(Centisecond, "ff", 2),
            new(Decisecond, "f"),
        };
        private static readonly FormatElement[] dateTimeElements = new FormatElement[]
        {

        };
        private static readonly FormatElement[] localDateTimeElements = new FormatElement[]
        {

        };
        public static FormatStringParser TimeDeltaFormatParser { get; } = new FormatStringParser(timeDeltaElements);
        public static FormatStringParser DateTimeFormatParser { get; } = new FormatStringParser(dateTimeElements);
        public static FormatStringParser LocalDateTimeFormatParser { get; } = new FormatStringParser(localDateTimeElements);

        public FormatStringParser(FormatElement[] elements) => this.elements = elements;

        public List<FormatElement> ParseSpecifiers(string input)
        {
            var ret = new List<FormatElement>();

            for (var i = 0; i < input.Length; i++)
            {
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
}
