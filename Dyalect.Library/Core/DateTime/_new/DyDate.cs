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

        public int Month => GetMonth(ticks);

        public int Day => GetDay(ticks);

        public bool IsLeapYear => Year % 4 == 0;

        private int GetYear(long ticks)
        {
            GetYearParts(ticks, out var primeTicks, out var leftTicks, out var primeYears);
            return (int)(leftTicks / TicksPerYear + primeYears + 1);
        }

        private long GetYearTicks(long ticks)
        {
            GetYearParts(ticks, out var primeTicks, out var leftTicks, out var _);
            return (leftTicks / TicksPerYear) * TicksPerYear + primeTicks;
        }

        private void GetYearParts(long ticks, out long primeTicks, out long leftTicks, out int primeYears)
        {
            var total4s = ticks / (DaysPer4Years * TicksPerDay);
            primeYears = (int)(total4s * 4);
            primeTicks = primeYears * TicksPerYear + total4s * TicksPerDay;
            leftTicks = ticks - primeTicks;
        }

        private int GetMonth(long ticks)
        {
            var days = (ticks - GetYearTicks(ticks)) / TicksPerDay;

            for (var i = 0; i < daysInMonths.Length; i++)
            {
                var dim = daysInMonths[i];

                if (i == 1 && IsLeapYear)
                    dim++;

                days -= dim;

                if (days < 0)
                    return i + 1;
            }

            throw new InvalidOperationException();
        }

        private int GetDay(long ticks)
        {
            var yt = GetYearTicks(ticks);
            var mt = ((ticks - GetYearTicks(ticks)) / TicksPerDay) * TicksPerDay;
            var day = (int)((ticks - yt - mt) / TicksPerDay);
            return day + 1;
        }
    }
}
