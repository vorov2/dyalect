using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Library.Core
{
    public sealed class DyDate
    {
        private readonly int days;

        public int Year => new DateTime(days * DT.TicksPerDay).Year;

        public int Month => new DateTime(days * DT.TicksPerDay).Month;

        public int Day => new DateTime(days * DT.TicksPerDay).Day;
    }

    public sealed class DyTime
    {
        private readonly long ticks;
        
        public int Tick => (int)(ticks % 10_000_000);

        public int Microsecond => (int)(ticks / DT.TicksPerMicrosecond % 1_000_000);

        public int Millisecond => (int)(ticks / DT.TicksPerMillisecond % 1000);

        public int Second => (int)(ticks / DT.TicksPerSecond % 60);

        public int Minute => (int)(ticks / DT.TicksPerMinute % 60);

        public int Hour => (int)(ticks / DT.TicksPerHour % 24);
    }

    public sealed class DyTestDateTime //: DyForeignObject
    {
        private const long TicksPerDay = 24 * TicksPerHour;
        private const long TicksPerHour = 60 * TicksPerMinute;
        private const long TicksPerMinute = 60 * TicksPerSecond;
        private const long TicksPerSecond = 10 * TicksPerDecisecond;
        private const long TicksPerDecisecond = 10 * TicksPerCentisecond;
        private const long TicksPerCentisecond = 10 * TicksPerMillisecond;
        private const long TicksPerMillisecond = 10 * TicksPerTenthThousandth;
        private const long TicksPerTenthThousandth = 10 * TicksPerHundredthThousandth;
        private const long TicksPerHundredthThousandth = 10 * TicksPerMicrosecond;
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

        public DyTestDateTime(long ticks) => this.ticks = ticks;

        public int Tick => (int)(ticks % 10_000_000);

        public int Microsecond => (int)(ticks / TicksPerMicrosecond % 1_000_000);

        public int Millisecond => (int)(ticks / TicksPerMillisecond % 1000);
        
        public int Second => (int)(ticks / TicksPerSecond % 60);

        public int Minute => (int)(ticks / TicksPerMinute % 60);

        public int Hour => (int)(ticks / TicksPerHour % 24);

        public long TotalTicks => ticks;




        public long TotalMinutes => ticks / TicksPerMinute;

        public long TotalHours => ticks / TicksPerHour;

        public int TotalDays => (int)(ticks / TicksPerDay);

        public int Year => DateOnly.FromDayNumber(TotalDays).Year;

        public int Month => DateOnly.FromDayNumber(TotalDays).Month;

        public int Day => DateOnly.FromDayNumber(TotalDays).Day;
    }
}
