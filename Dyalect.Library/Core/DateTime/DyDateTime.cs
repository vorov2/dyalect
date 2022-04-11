using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public class DyDateTime : DyForeignObject
    {
        internal readonly DateTime Value;
        internal readonly TimeSpan? Offset;

        internal DyDateTime(DyBaseDateTimeTypeInfo typeInfo, DateTime value) : base(typeInfo) =>
            Value = value;

        protected DyDateTime(DyBaseDateTimeTypeInfo typeInfo, DateTime value, TimeSpan offset) : base(typeInfo) =>
            (Value, Offset) = (value, offset);

        public override object ToObject() => Value;

        public override DyObject Clone() => this;

        public override bool Equals(DyObject? other) => other is DyDateTime dt && dt.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();

        public long GetTicks()
        {
            if (Offset is null)
                return Value.Ticks;

            var tdo = new DateTimeOffset(DateTime.SpecifyKind(Value, DateTimeKind.Unspecified), Offset.Value);
            return tdo.Ticks;
        }
    }
}
