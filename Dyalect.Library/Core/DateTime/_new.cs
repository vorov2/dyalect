using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Library.Core;

public class _DyDateTime : IDateTime
{
    private long ticks;

    long ISpan.TotalTicks => ticks;

    int ITime.Ticks => (int)(ticks % 10_000_000);

    int ITime.Microseconds => (int)(ticks / DT.TicksPerMicrosecond % 1_000_000);

    int ITime.Milliseconds => (int)(ticks / DT.TicksPerMillisecond % 1000);

    int ITime.Seconds => (int)(ticks / DT.TicksPerSecond % 60);

    int ITime.Minutes => (int)(ticks / DT.TicksPerMinute % 60);

    int ITime.Hours => (int)(ticks / DT.TicksPerHour % 24);

    int IDate.Year => new DateTime(ticks).Year;

    int IDate.Month => new DateTime(ticks).Month;

    int IDate.Day => new DateTime(ticks).Day;

    string IDate.DayOfWeek => new DateTime(ticks).DayOfWeek.ToString();

    int IDate.DayOfYear => new DateTime(ticks).DayOfYear;

    void IDate.AddDays(int value) => SetTicks(new DateTime(ticks).AddDays(value));

    void IDate.AddMonths(int value) => SetTicks(new DateTime(ticks).AddMonths(value));

    void IDate.AddYears(int value) => SetTicks(new DateTime(ticks).AddYears(value));

    void IDateTime.AddHours(double value) => SetTicks(new DateTime(ticks).AddHours(value));

    void IDateTime.AddMinutes(double value) => SetTicks(new DateTime(ticks).AddMinutes(value));

    void IDateTime.AddSeconds(double value) => SetTicks(new DateTime(ticks).AddSeconds(value));

    void IDateTime.AddMilliseconds(double value) => SetTicks(new DateTime(ticks).AddMilliseconds(value));

    void IDateTime.AddTicks(long value) => ticks += value;

    DyObject IDateTime.GetDate(DyDateTypeInfo typeInfo) =>
        new DyDate(typeInfo, DateOnly.FromDateTime(new DateTime(ticks)).DayNumber);

    DyObject IDateTime.GetTime(DyTimeTypeInfo typeInfo) =>
        new DyTime(typeInfo, TimeOnly.FromDateTime(new DateTime(ticks)).Ticks);

    private void SetTicks(DateTime dt) => ticks = dt.Ticks;
}

//public sealed class _DyDateTimeTypeInfo : AbstractDateTimeTypeInfo<_DyDateTime>
//{

//}
