using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Library.Core
{
    public sealed class DyTime //: DyForeignObject, ITime
    {
        private const string DEFAULT_FORMAT = "hh:mm:ss.fffffff";

        private readonly long ticks;

        public long TotalTicks => ticks;

        public int Ticks => (int)(ticks % 10_000_000);

        public int Microseconds => (int)(ticks / DT.TicksPerMicrosecond % 1_000_000);

        public int Milliseconds => (int)(ticks / DT.TicksPerMillisecond % 1000);

        public int Seconds => (int)(ticks / DT.TicksPerSecond % 60);

        public int Minutes => (int)(ticks / DT.TicksPerMinute % 60);

        public int Hours => (int)(ticks / DT.TicksPerHour % 24);
    }
}
