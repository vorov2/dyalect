using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Library.Core
{
    public sealed class DyTime : DyForeignObject, ITime
    {
        private const string DEFAULT_FORMAT = "hh:mm:ss.fffffff";

        private readonly long ticks;

        public DyTime(DyTimeTypeInfo typeInfo, long ticks) : base(typeInfo) => this.ticks = ticks;

        public long TotalTicks => ticks;

        public int Ticks => (int)(ticks % 10_000_000);

        public int Microseconds => (int)(ticks / DT.TicksPerMicrosecond % 1_000_000);

        public int Milliseconds => (int)(ticks / DT.TicksPerMillisecond % 1000);

        public int Seconds => (int)(ticks / DT.TicksPerSecond % 60);

        public int Minutes => (int)(ticks / DT.TicksPerMinute % 60);

        public int Hours => (int)(ticks / DT.TicksPerHour % 24);

        public override object ToObject() => new TimeOnly(ticks);

        public override DyObject Clone() => this;

        public override int GetHashCode() => ticks.GetHashCode();

        public override bool Equals(DyObject? other) => other is DyTime dt && dt.ticks == ticks;
        
        public static DyTime Parse(DyTimeTypeInfo typeInfo, string format, string value)
        {
            var ticks = DT.Parse(FormatParser.TimeParser, format, value);
            return new(typeInfo, ticks);
        }

        public override string ToString() => ToString(DEFAULT_FORMAT);

        public string ToString(string format)
        {
            var formats = FormatParser.TimeParser.ParseSpecifiers(format);
            var sb = new StringBuilder();

            foreach (var f in formats)
                DT.FormatTime(this, sb, f);

            return sb.ToString();
        }
    }
}
