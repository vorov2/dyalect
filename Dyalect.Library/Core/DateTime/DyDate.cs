using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyDate : DyForeignObject, IDate, IFormattable
    {
        private const string DEFAULT_FORMAT = "yyyy-MM-dd";

        private readonly int days;

        public DyDate(DyTimeTypeInfo typeInfo, int days) : base(typeInfo) => this.days = days;

        public int Year => new DateTime(days * DT.TicksPerDay).Year;

        public int Month => new DateTime(days * DT.TicksPerDay).Month;

        public int Day => new DateTime(days * DT.TicksPerDay).Day;


        public override object ToObject() => new DateOnly(Year, Month, Day);

        public override DyObject Clone() => this;

        public override int GetHashCode() => days.GetHashCode();

        public override bool Equals(DyObject? other) => other is DyDate dt && dt.days == days;

        public static DyTime Parse(DyTimeTypeInfo typeInfo, string format, string value)
        {
            var ticks = DT.Parse(FormatParser.TimeParser, format, value);
            return new(typeInfo, ticks);
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
