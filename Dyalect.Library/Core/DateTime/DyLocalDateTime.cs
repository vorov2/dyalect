using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyLocalDateTime : DyDateTime
    {
        private const string FORMAT = "yyyy-MM-dd HH:mm:ss.fffffff zzz";
        
        public DyTimeDelta Offset { get; }

        internal DyLocalDateTime(DyLocalDateTimeTypeInfo typeInfo, long ticks, DyTimeDelta offset)
            : base(typeInfo, ticks) => this.Offset = offset;

        public override bool Equals(DyObject? other) => other is DyLocalDateTime dt
            && dt.ticks == ticks && dt.Offset.Equals(Offset);

        public static new DyDateTime Parse(DyForeignTypeInfo typeInfo, string format, string value)
        {
            var (ticks, _) = InputParser.Parse(FormatParser.LocalDateTimeParser, format, value);
            return new((AbstractDateTimeTypeInfo<DyDateTime>)typeInfo, ticks);
        }

        public override DyObject Clone() => 
            new DyLocalDateTime((DyLocalDateTimeTypeInfo)TypeInfo, ticks, Offset);

        public override int GetHashCode() => ticks.GetHashCode();

        public override string ToString(string? format, IFormatProvider? _ = null)
        {
            var formats = FormatParser.DateTimeParser.ParseSpecifiers(format ?? FORMAT);
            var sb = new StringBuilder();

            foreach (var f in formats)
                Formatter.FormatDateTime(this, sb, f);

            return sb.ToString();
        }

        public override object ToObject() => new DateTimeOffset(new DateTime(ticks, DateTimeKind.Unspecified), Offset.ToTimeSpan());
    }
}
