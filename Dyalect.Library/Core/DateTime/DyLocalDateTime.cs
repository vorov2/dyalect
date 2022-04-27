using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyLocalDateTime : DyDateTime, ILocalDateTime
    {
        private const string FORMAT = "yyyy-MM-dd HH:mm:ss.fffffffzzz";
        
        public DyTimeDelta Offset { get; }

        IInterval ILocalDateTime.Interval => Offset;

        internal DyLocalDateTime(DyLocalDateTimeTypeInfo typeInfo, long ticks, DyTimeDelta offset)
            : base(typeInfo, ticks) => this.Offset = offset;

        public override bool Equals(DyObject? other) => other is DyLocalDateTime dt
            && dt.ticks == ticks && dt.Offset.Equals(Offset);

        public static new DyDateTime Parse(DyForeignTypeInfo typeInfo, string format, string value)
        {
            var ti = (DyLocalDateTimeTypeInfo)typeInfo;
            var (ticks, offset) = InputParser.Parse(FormatParser.LocalDateTimeParser, format, value);
            return new DyLocalDateTime(ti, ticks,
                new DyTimeDelta(ti.TypeDeltaTypeInfo, offset == 0 ? TimeZoneInfo.Local.BaseUtcOffset : TimeSpan.FromTicks(offset)));
        }

        public override DyObject Clone() => 
            new DyLocalDateTime((DyLocalDateTimeTypeInfo)TypeInfo, ticks, Offset);

        public override int GetHashCode() => ticks.GetHashCode();

        public override string ToString() => ToString(FORMAT);

        public override string ToString(string? format, IFormatProvider? _ = null)
        {
            var formats = FormatParser.LocalDateTimeParser.ParseSpecifiers(format ?? FORMAT);
            var sb = new StringBuilder();

            foreach (var f in formats)
                Formatter.FormatLocalDateTime(this, sb, f);

            return sb.ToString();
        }

        public override object ToObject() => ToDateTimeOffset();

        public DateTimeOffset ToDateTimeOffset() => new(new DateTime(ticks, DateTimeKind.Unspecified), Offset.ToTimeSpan());

        public override DyDateTime FirstDayOfMonth()
        {
            var dt = new DateTime(ticks, DateTimeKind.Unspecified);
            return new DyLocalDateTime((DyLocalDateTimeTypeInfo)TypeInfo, dt.AddDays(-dt.Day + 1).Ticks, Offset);
        }

        public override DyDateTime LastDayOfMonth()
        {
            var dt = new DateTime(ticks, DateTimeKind.Unspecified);
            return new DyLocalDateTime((DyLocalDateTimeTypeInfo)TypeInfo, dt.AddDays(DateTime.DaysInMonth(dt.Year, dt.Month) - dt.Day).Ticks, Offset);
        }
    }
}
