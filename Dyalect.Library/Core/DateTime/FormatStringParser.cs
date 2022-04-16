using System.Collections.Generic;
using static Dyalect.Library.Core.FormatElementKind;

namespace Dyalect.Library.Core
{
    internal enum FormatElementKind
    {
        Literal,
        d,
        dd,
        ddd,
        dddd,
        h,
        hh,
        H,
        HH,
        m,
        mm,
        M,
        MM,
        MMM,
        MMMM,
        s,
        ss,
        t,
        tt,
        f,
        ff,
        fff,
        ffff,
        fffff,
        ffffff,
        fffffff,
        F,
        FF,
        FFF,
        FFFF,
        FFFFF,
        FFFFFF,
        FFFFFFF,
        y,
        yy,
        yyy,
        yyyy,
        yyyyy,
        g,
        K,
        z,
        zz,
        zzz
    }

    internal record FormatElement(FormatElementKind Kind, string Value);

    internal sealed class FormatStringParser
    {
        private readonly FormatElement[] elements;
        private static readonly FormatElement[] timeDeltaElements = new FormatElement[] 
        {
            new(d, "d"),
            new(dd, "dd"),
            new(h, "h"),
            new(hh, "hh"),
            new(m, "m"),
            new(mm, "mm"),
            new(s, "s"),
            new(ss, "ss"),
            new(f, "f"),
            new(ff, "ff"),
            new(fff, "fff"),
            new(ffff, "ffff"),
            new(fffff, "fffff"),
            new(ffffff, "ffffff"),
            new(fffffff, "fffffff"),
            new(F, "F"),
            new(FF, "FF"),
            new(FFF, "FFF"),
            new(FFFF, "FFFF"),
            new(FFFFF, "FFFFF"),
            new(FFFFFF, "FFFFFF"),
            new(FFFFFFF, "FFFFFFF"),
        };
        private static readonly FormatElement[] dateTimeElements = new FormatElement[]
        {
            new(d, "d"),
            new(dd, "dd"),
            new(ddd, "ddd"),
            new(dddd, "dddd"),
            new(M, "M"),
            new(MM, "MM"),
            new(MMM, "MMM"),
            new(MMMM, "MMMM"),
            new(m, "m"),
            new(mm, "mm"),
            new(h, "h"),
            new(hh, "hh"),
            new(H, "H"),
            new(HH, "HH"),
            new(g, "g"),
            new(f, "f"),
            new(ff, "ff"),
            new(fff, "fff"),
            new(ffff, "ffff"),
            new(fffff, "fffff"),
            new(ffffff, "ffffff"),
            new(fffffff, "fffffff"),
            new(F, "F"),
            new(FF, "FF"),
            new(FFF, "FFF"),
            new(FFFF, "FFFF"),
            new(FFFFF, "FFFFF"),
            new(FFFFFF, "FFFFFF"),
            new(FFFFFFF, "FFFFFFF"),
            new(s, "s"),
            new(ss, "ss"),
            new(t, "t"),
            new(tt, "tt"),
            new(t, "t"),
            new(tt, "tt"),
            new(y, "y"),
            new(yy, "yy"),
            new(yyy, "yyy"),
            new(yyyy, "yyyy"),
            new(yyyyy, "yyyyy"),
        };
        private static readonly FormatElement[] localDateTimeElements = new FormatElement[]
        {
            new(d, "d"),
            new(dd, "dd"),
            new(ddd, "ddd"),
            new(dddd, "dddd"),
            new(M, "M"),
            new(MM, "MM"),
            new(MMM, "MMM"),
            new(MMMM, "MMMM"),
            new(m, "m"),
            new(mm, "mm"),
            new(h, "h"),
            new(hh, "hh"),
            new(H, "H"),
            new(HH, "HH"),
            new(g, "g"),
            new(f, "f"),
            new(ff, "ff"),
            new(fff, "fff"),
            new(ffff, "ffff"),
            new(fffff, "fffff"),
            new(ffffff, "ffffff"),
            new(fffffff, "fffffff"),
            new(F, "F"),
            new(FF, "FF"),
            new(FFF, "FFF"),
            new(FFFF, "FFFF"),
            new(FFFFF, "FFFFF"),
            new(FFFFFF, "FFFFFF"),
            new(FFFFFFF, "FFFFFFF"),
            new(s, "s"),
            new(ss, "ss"),
            new(t, "t"),
            new(tt, "tt"),
            new(t, "t"),
            new(tt, "tt"),
            new(y, "y"),
            new(yy, "yy"),
            new(yyy, "yyy"),
            new(yyyy, "yyyy"),
            new(yyyyy, "yyyyy"),
            new(z, "z"),
            new(zz, "zz"),
            new(zzz, "zzz"),
            new(K, "K"),
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
                for (var j = 0; j < elements.Length; j++)
                    if (CheckSpecifier(input, elements[i].Value, ref i))
                    {
                        ret.Add(elements[i]);
                        continue;
                    }

                ret.Add(new FormatElement(Literal, input[i].ToString()));
            }

            return ret;
        }

        private bool CheckSpecifier(string input, string specifier, ref int index)
        {
            var preserved = index;

            if (specifier.Length == 1)
                return input[index] == specifier[0];
            
            for (var i = 0; i < specifier.Length; i++)
                if (specifier[i] != input[index++])
                {
                    index = preserved;
                    return false;
                }

            return true;
        }
    }
}
