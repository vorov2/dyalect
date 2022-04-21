using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyDate : DyForeignObject, IDate, ISpan, IFormattable
    {
        private const string DEFAULT_FORMAT = "yyyy-MM-dd";

        private readonly int days;

        public DyDate(DyDateTypeInfo typeInfo, int days) : base(typeInfo) => this.days = days;

        long ISpan.TotalTicks => days * DT.TicksPerDay;

        public int Year => new DateTime(days * DT.TicksPerDay).Year;

        public int Month => new DateTime(days * DT.TicksPerDay).Month;

        public int Day => new DateTime(days * DT.TicksPerDay).Day;

        public override object ToObject() => new DateOnly(Year, Month, Day);

        public override DyObject Clone() => this;

        public override int GetHashCode() => days.GetHashCode();

        public override bool Equals(DyObject? other) => other is DyDate dt && dt.days == days;

        public static DyDate Parse(DyDateTypeInfo typeInfo, string format, string value)
        {
            var ticks = DT.Parse(FormatParser.DateParser, format, value);
            return new(typeInfo, (int)(ticks / DT.TicksPerDay));
        }

        public string ToString(string? format, IFormatProvider? _ = null)
        {
            var formats = FormatParser.DateParser.ParseSpecifiers(format ?? DEFAULT_FORMAT);
            var sb = new StringBuilder();

            foreach (var f in formats)
                DT.FormatDate(this, sb, f);

            return sb.ToString();
        }

        public override string ToString() => ToString(DEFAULT_FORMAT);
    }
}
