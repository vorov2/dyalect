using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyTime : DyForeignObject, ITime, IFormattable
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

        public int Hours => (int)(ticks / DT.TicksPerHour);

        public override object ToObject() => new TimeOnly(ticks);

        public override DyObject Clone() => this;

        public override int GetHashCode() => ticks.GetHashCode();

        public override bool Equals(DyObject? other) => other is DyTime dt && dt.ticks == ticks;
        
        public static DyTime Parse(DyTimeTypeInfo typeInfo, string format, string value)
        {
            var (ticks, _) = InputParser.Parse(FormatParser.TimeParser, format, value);
            return new(typeInfo, ticks);
        }

        public string ToString(string? format, IFormatProvider? _ = null)
        {
            var formats = FormatParser.TimeParser.ParseSpecifiers(format ?? DEFAULT_FORMAT);
            var sb = new StringBuilder();

            foreach (var f in formats)
                Formatter.FormatTime(this, sb, f);

            return sb.ToString();
        }

        public override string ToString() => ToString(DEFAULT_FORMAT);
    }
}
