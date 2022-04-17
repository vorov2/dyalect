using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyLocalDateTime : DyDateTime
    {
        internal DyLocalDateTime(DyBaseDateTimeTypeInfo typeInfo, DateTime value, TimeSpan offset)
            : base(typeInfo, value, offset) { }

        public override bool Equals(DyObject? other) => other is DyLocalDateTime dt
            && dt.Value == Value && dt.Offset == Offset;

        public override object ToObject() => new DateTimeOffset(DateTime.SpecifyKind(Value, DateTimeKind.Unspecified), Offset ?? default);

        internal override DyDateTime ChangeDay(int day) => new DyLocalDateTime((DyBaseDateTimeTypeInfo)TypeInfo,
            new DateTime(Value.Year, Value.Month, day, Value.Hour, Value.Minute, Value.Second, Value.Millisecond, DateTimeKind.Local), Offset!.Value);
    }
}
