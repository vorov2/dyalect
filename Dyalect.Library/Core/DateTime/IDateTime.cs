using Dyalect.Runtime.Types;

namespace Dyalect.Library.Core;

public interface ISpan
{
    long TotalTicks { get; }

    long ToInteger();
}

public interface IDate : ISpan
{
    int Year { get; }

    int Month { get; }

    int Day { get; }

    int DayOfYear { get; }

    string DayOfWeek { get; }

    void AddDays(int value);

    void AddMonths(int value);

    void AddYears(int value);
}

public interface IDateTime : IDate, ITime
{
    DyObject GetDate(DyDateTypeInfo typeInfo);

    DyObject GetTime(DyTimeTypeInfo typeInfo);

    void AddHours(double value);

    void AddMinutes(double value);

    void AddSeconds(double value);

    void AddMilliseconds(double value);

    void AddTicks(long value);
}

public interface ITime : ISpan
{
    int Hours { get; }

    int Minutes { get; }

    int Seconds { get; }

    int Milliseconds { get; }

    int Microseconds { get; }

    int Ticks { get; }
}

public interface IInterval : ITime
{
    int Days { get; }
}
