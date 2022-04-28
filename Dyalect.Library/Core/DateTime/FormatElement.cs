namespace Dyalect.Library.Core;

internal enum FormatElementKind
{
    Literal,
    Sign,
    Year,
    Month,
    MonthAbbrev,
    MonthName,
    Day,
    Hour,
    Hour24,
    Minute,
    Second,
    Decisecond,
    Centisecond,
    Millisecond,
    TenthThousandth,
    HundredthThousandth,
    Microsecond,
    Tick,
    PmAm,
    Offset
}

internal record FormatElement(FormatElementKind Kind, string Value, int Padding = 1);

