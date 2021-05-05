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

        Pop = 0x10,

        Function = 0x20,

        IteratorBody = 0x40,

        Last = 0x80,

        Rebind = 0x100,

        ExpectPush = 0x200,

        OpenMatch = 0x400,

        NoScope = 0x800,

        Catch = 0x1000,

        NoBinding = 0x2000
    }

    public static class HintsExtensions
    {
        public static bool Has(this Hints hints, Hints val) => (hints & val) == val;

        public static Hints Push(this Hints hints)
        {
            if ((hints & Hints.Push) != Hints.Push)
                return hints | Hints.Push;
            else
                return hints;
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
