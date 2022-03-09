using Dyalect.Runtime.Types;

namespace Dyalect.Library.Types
{
    public sealed class DyResult : DyForeignObject
    {
        internal readonly DyObject Value;

        public DyResult(DyForeignTypeInfo typeInfo, string ctor, DyObject value) : base(typeInfo, ctor) =>
            Value = value;

        public override object ToObject() => Value.ToObject();
    }
}
