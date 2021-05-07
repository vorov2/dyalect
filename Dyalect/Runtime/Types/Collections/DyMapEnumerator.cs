using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyMapEnumerator : IEnumerator<DyObject>
    {
        private readonly DyMap obj;
        private readonly IEnumerator enumerator;
        private readonly int version;

        public DyMapEnumerator(DyMap obj)
        {
            this.obj = obj;
            version = obj.Version;
            enumerator = obj.Map.GetEnumerator();
        }

        public DyObject Current
        {
            get
            {
                var obj = (KeyValuePair<DyObject, DyObject>)enumerator.Current;
                return new DyTuple(new DyObject[] {
                        new DyLabel("key", obj.Key),
                        new DyLabel("value", obj.Value)
                        });
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext() =>
            version != obj.Version ? throw new IterationException() : enumerator.MoveNext();

        public void Reset() => enumerator.Reset();
    }
}
