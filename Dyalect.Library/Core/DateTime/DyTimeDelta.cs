using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeDelta : DyForeignObject, IInterval, IFormattable
    {
        private const string DEFAULT_FORMAT = "+d.hh:mm:ss.fffffff";

        private readonly long ticks;

        public long TotalTicks => ticks;

        public int Ticks => (int)(ticks % 10_000_000);

        public int Microseconds => (int)(ticks / DT.TicksPerMicrosecond % 1_000_000);

        public int Milliseconds => (int)(ticks / DT.TicksPerMillisecond % 1000);

        public int Seconds => (int)(ticks / DT.TicksPerSecond % 60);

        public int Minutes => (int)(ticks / DT.TicksPerMinute % 60);

        public int Hours => (int)(ticks / DT.TicksPerHour % 24);

        public int Days => (int)(ticks / DT.TicksPerDay);

        public DyTimeDelta(DyTimeDeltaTypeInfo typeInfo, long ticks) : base(typeInfo) => this.ticks = ticks;

        public DyTimeDelta(DyTimeDeltaTypeInfo typeInfo, TimeSpan timeSpan) : this(typeInfo, timeSpan.Ticks) { }
        
        public override object ToObject() => ToTimeSpan();

        public TimeSpan ToTimeSpan() => TimeSpan.FromTicks(ticks);

        public DyTimeDelta Negate() => new((DyTimeDeltaTypeInfo)TypeInfo, -ticks);

        public static DyTimeDelta Parse(DyTimeDeltaTypeInfo typeInfo, string format, string value)
        {
            var (ticks, _) = InputParser.Parse(FormatParser.TimeDeltaParser, format, value);
            return new(typeInfo, ticks);
        }

        public string ToString(string? format, IFormatProvider? _ = null)
        {
            var formats = FormatParser.TimeDeltaParser.ParseSpecifiers(format ?? DEFAULT_FORMAT);
            var sb = new StringBuilder();

            foreach (var f in formats)
                DT.FormatInterval(this, sb, f);

            return sb.ToString();
        }
        
        public override string ToString() => ToString(DEFAULT_FORMAT);

        public override int GetHashCode() => ticks.GetHashCode();

        public override bool Equals(DyObject? other) => other is DyTimeDelta d && d.ticks == ticks;

        public override DyObject Clone() => this;
    }
}
