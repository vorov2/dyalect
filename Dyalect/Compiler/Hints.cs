using System;

namespace Dyalect.Compiler
{
    [Flags]
    public enum Hints
    {
        None = 0x00,

        Push = 0x01,

        Const = 0x02,

        Declare = 0x04,

        ExplicitBind = 0x08,

        Function = 0x10,

        Pop = 0x20
    }

    public static class HintsExtensions
    {
        public static bool Has(this Hints hints, Hints val)
        {
            return (hints & val) == val;
        }

        public static Hints Append(this Hints hints, Hints newHint)
        {
            if ((hints & newHint) != newHint)
                return hints | newHint;
            else
                return hints;
        }

        public static Hints Remove(this Hints hints, Hints newHint)
        {
            if ((hints & newHint) == newHint)
                return hints ^ newHint;
            else
                return hints;
        }
    }
}
