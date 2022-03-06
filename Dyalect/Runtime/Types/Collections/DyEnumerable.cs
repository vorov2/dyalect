using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public abstract class DyEnumerable : DyObject, IEnumerable<DyObject>
    {
        public int Version { get; protected set; }

        public virtual int Count { get; protected set; }

        protected DyEnumerable(int typeCode) : base(typeCode) { }

        public abstract IEnumerator<DyObject> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override int GetHashCode() => base.CalculateSimpleHashCode();
    }
}
