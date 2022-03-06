using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyForeignIterator : DyIterator
    {
        private readonly IEnumerable<DyObject> seq;

        public DyForeignIterator(DyTypeInfo typeInfo, IEnumerable<DyObject> seq) : base(typeInfo) => this.seq = seq;

        public override DyFunction GetIteratorFunction() => new DyIteratorFunction(DecType, seq);

        public override object ToObject() => seq;

        public override int GetHashCode() => seq.GetHashCode();
    }
}
