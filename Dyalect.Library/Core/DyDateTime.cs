using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    internal class DyDateTime : DyForeignObject
    {
        internal readonly DateTime Value;

        public DyDateTime(DyDateTimeTypeInfo typeInfo, DateTime value) : base(typeInfo) => Value = value;

        public override object ToObject() => Value;
    }
}
