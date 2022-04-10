using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Library.Core
{
    public sealed class DyLocalDateTime : DyDateTime
    {
        internal DyLocalDateTime(DyBaseDateTimeTypeInfo typeInfo, DateTime value, TimeSpan offset)
            : base(typeInfo, value) => Offset = offset;

        public override bool Equals(DyObject? other) => other is DyLocalDateTime dt
            && dt.Value == Value && dt.Offset == Offset;
    }
}
