using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Library.Core
{
    public sealed class DyGuid : DyForeignObject
    {
        internal readonly Guid Value;

        public DyGuid(DyGuidTypeInfo typeInfo, Guid value) : base(typeInfo) => Value = value;

        public override int GetHashCode() => Value.GetHashCode();

        public override object ToObject() => Value;

        public override string ToString() => Value.ToString();
    }
}
