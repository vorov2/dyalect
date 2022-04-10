using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyDateTime : DyForeignObject
    {
        public readonly DateTime Value;

        public DyDateTime(DyDateTimeTypeInfo typeInfo, DateTime value) : base(typeInfo) => 
            Value = value;

        public override object ToObject() => Value;

        public override DyObject Clone() => this;

        public override bool Equals(DyObject? other) => other is DyDateTime dt && dt.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();
    }
}
