using System;

namespace Dyalect.UnitTesting
{
    [Flags]
    public enum TestFormatFlags
    {
        None = 0x00,

        OnlyFailed = 0x01,

        Markdown = 0x02
    }
}
