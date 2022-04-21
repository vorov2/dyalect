using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeDelta : DyForeignObject, IInterval
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

        public DyTimeDelta Negate() => new DyTimeDelta((DyTimeDeltaTypeInfo)TypeInfo, -ticks);

        public static DyTimeDelta Parse(DyTimeDeltaTypeInfo typeInfo, string format, string value)
        {
            var formats = FormatParser.TimeDeltaParser.ParseSpecifiers(format);
            var chunks = InputParser.Parse(formats, value);
            var (days, hours, minutes, seconds, ds, cs, ms, tts, hts, micros, tick) =
                (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var negate = false;
            
            foreach (var (kind, val) in chunks)
                switch (kind)
                {
                    case FormatElementKind.Sign:
                        negate = val == "-";
                        break;
                    case FormatElementKind.Day:
                        if (!int.TryParse(val, out days)) throw new FormatException();
                        break;
                    case FormatElementKind.Hour:
                        if (!int.TryParse(val, out hours)) throw new FormatException();
                        if (hours is < 0 or > 23) throw new OverflowException();
                        break;
                    case FormatElementKind.Minute:
                        if (!int.TryParse(val, out minutes)) throw new FormatException();
                        if (minutes is < 0 or > 59) throw new OverflowException();
                        break;
                    case FormatElementKind.Second:
                        if (!int.TryParse(val, out seconds)) throw new FormatException();
                        if (seconds is < 0 or > 59) throw new OverflowException();
                        break;
                    case FormatElementKind.Decisecond:
                        if (!int.TryParse(val, out ds)) throw new FormatException();
                        if (ds is < 0 or > 9) throw new OverflowException();
                        break;
                    case FormatElementKind.Centisecond:
                        if (!int.TryParse(val, out cs)) throw new FormatException();
                        if (cs is < 0 or > 99) throw new OverflowException();
                        break;
                    case FormatElementKind.Millisecond:
                        if (!int.TryParse(val, out ms)) throw new FormatException();
                        if (ms is < 0 or > 999) throw new OverflowException();
                        break;
                    case FormatElementKind.TenthThousandth:
                        if (!int.TryParse(val, out tts)) throw new FormatException();
                        if (tts is < 0 or > 9_999) throw new OverflowException();
                        break;
                    case FormatElementKind.HundredthThousandth:
                        if (!int.TryParse(val, out hts)) throw new FormatException();
                        if (hts is < 0 or > 99_999) throw new OverflowException();
                        break;
                    case FormatElementKind.Microsecond:
                        if (!int.TryParse(val, out micros)) throw new FormatException();
                        if (micros is < 0 or > 999_999) throw new OverflowException();
                        break;
                    case FormatElementKind.Tick:
                        if (!int.TryParse(val, out tick)) throw new FormatException();
                        if (tick is < 0 or > 9_999_999) throw new OverflowException();
                        break;
                }

            var totalTicks =
                tick +
                micros * DT.TicksPerMicrosecond +
                hts * DT.TicksPerMillisecond * 100 +
                tts * DT.TicksPerMillisecond * 10 +
                ms * DT.TicksPerMillisecond +
                cs * DT.TicksPerCentisecond +
                ds * DT.TicksPerDecisecond +
                seconds * DT.TicksPerSecond +
                minutes * DT.TicksPerMinute +
                hours * DT.TicksPerHour +
                days * DT.TicksPerDecisecond;

            if (negate)
                totalTicks = -totalTicks;

            return new DyTimeDelta(typeInfo, totalTicks);
        }

        public string ToString(string format)
        {
            var formats = FormatParser.TimeDeltaParser.ParseSpecifiers(format);
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
