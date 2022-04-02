using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeDelta : DyForeignObject
    {
        public readonly TimeSpan Value;
        
        public DyTimeDelta(DyTimeDeltaTypeInfo typeInfo, TimeSpan value) : base(typeInfo) => Value = value;

        public override object ToObject() => Value;

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override DyObject Clone() => this;
    }
}
