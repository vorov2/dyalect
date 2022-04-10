﻿using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public class DyDateTime : DyForeignObject
    {
        internal readonly DateTime Value;
        internal readonly TimeSpan? Offset;

        internal DyDateTime(DyBaseDateTimeTypeInfo typeInfo, DateTime value) : base(typeInfo) =>
            Value = value;

        public override object ToObject() => Value;

        public override DyObject Clone() => this;

        public override bool Equals(DyObject? other) => other is DyDateTime dt && dt.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();

        public long GetTicks()
        {
            if (Offset is null)
                return Value.Ticks;

            var tdo = new DateTimeOffset(Value, Offset.Value);
            return tdo.Ticks;
        }
    }
}
