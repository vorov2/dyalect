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
    }
}
