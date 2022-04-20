using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Library.Core.DateTime
{
    public sealed class DyTime //: DyForeignObject
    {
        private const int DaysPer400Years = DaysPer100Years * 4 + 1;
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;
        private const int DaysPer4Years = DaysPerYear * 4 + 1;
        private const int DaysPerYear = 365;

        private const long TicksPerYear = DaysPerYear * TicksPerDay;

        private const long TicksPerDay = 24 * TicksPerHour;
        private const long TicksPerHour = 60 * TicksPerMinute;
        private const long TicksPerMinute = 60 * TicksPerSecond;
        private const long TicksPerSecond = 10 * TicksPerDecisecond;
        private const long TicksPerDecisecond = 10 * TicksPerCentisecond;
        private const long TicksPerCentisecond = 10 * TicksPerMillisecond;
        private const long TicksPerMillisecond = 1000 * TicksPerMicrosecond;
        private const long TicksPerMicrosecond = 10L;

        private readonly int[] daysInMonths = new[]
        {
            31, //Jan
            28, //Feb
            31, //Mar
            30, //Apr
            31, //May
            30, //Jun
            31, //Jul
            31, //Aug
            30, //Sep
            31, //Oct
            30, //Nov
            31  //Dec
        };

        private readonly long ticks;

        public int Tick => (int)(ticks % 10_000_000);

        public int Microsecond => (int)(ticks / TicksPerMicrosecond % 1_000_000);

        public int Millisecond => (int)(ticks / TicksPerMillisecond % 1000);

        public int Second => (int)(ticks / TicksPerSecond % 60);

        public int Minute => (int)(ticks / TicksPerMinute % 60);

        public int Hour => (int)(ticks / TicksPerHour % 24);

        public long TotalTicks => ticks;

        public long TotalMicroseconds => ticks / TicksPerMicrosecond;

        public long TotalMilliseconds => ticks / TicksPerMillisecond;

        public long TotalSeconds => ticks / TicksPerSecond;

        public long TotalMinutes => ticks / TicksPerMinute;

        public long TotalHours => ticks / TicksPerHour;

        public int Year => GetYear(ticks);

        public int Month 
        {
            get
            {
                GetMonth(ticks, out var month, out _);
                return month;
            }
        }

        public int Day
        {
            get
            {
                GetMonth(ticks, out _, out var day);
                return day;
            }
        }

        public bool IsLeapYear => Year % 4 == 0;

        private int GetYear(long ticks)
        {
            var total4s = (ticks + TicksPerYear + TicksPerDay) / (DaysPer4Years * TicksPerDay);

            var total100s = ticks / (DaysPer100Years * TicksPerDay);
            var total400s = ticks / (DaysPer400Years * TicksPerDay);
            var totalLeaps = total4s - (total100s - total400s);

            var primeYears = (int)(totalLeaps * 4);
            var primeTicks = primeYears * TicksPerYear + totalLeaps * TicksPerDay;
            var leftTicks = ticks - primeTicks;
            return (int)(leftTicks / TicksPerYear + primeYears + 1);
        }

        private long GetYearTicks(long ticks)
        {

            var total4s = (ticks + TicksPerYear + TicksPerDay) / (DaysPer4Years * TicksPerDay);

            var total100s = ticks / (DaysPer100Years * TicksPerDay);
            var total400s = ticks / (DaysPer400Years * TicksPerDay);
            var totalLeaps = total4s - (total100s - total400s);

            var primeYears = (int)(totalLeaps * 4);
            var primeTicks = primeYears * TicksPerYear + totalLeaps * TicksPerDay;
            var leftTicks = ticks - primeTicks;
            return (leftTicks / TicksPerYear) * TicksPerYear + primeTicks;
        }

        private void GetMonth(long ticks, out int month, out int day)
        {
            var days = (int)((ticks - GetYearTicks(ticks)) / TicksPerDay);
            month = 0;
            day = 0;

            for (var i = 0; i < daysInMonths.Length; i++)
            {
                var dim = daysInMonths[i];

                if (i == 1 && IsLeapYear)
                    dim++;

                days -= dim;

                if (days < 0)
                {
                    month = i + 1;
                    day = days + dim + 1;
                    return;
                }
            }

            throw new InvalidOperationException();
        }
    }
}
