using Dyalect.Runtime.Types;
namespace Dyalect.Library.Core;

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
