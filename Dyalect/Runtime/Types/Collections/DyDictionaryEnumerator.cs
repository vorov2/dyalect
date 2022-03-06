using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyDictionaryEnumerator : IEnumerator<DyObject>
    {
        private readonly DyDictionary obj;
        private readonly IEnumerator enumerator;
        private readonly int version;
        private readonly RuntimeContext rtx;

        public DyDictionaryEnumerator(RuntimeContext rtx, DyDictionary obj)
        {
            this.rtx = rtx;
            this.obj = obj;
            version = obj.Version;
            enumerator = obj.Map.GetEnumerator();
        }

        public DyObject Current
        {
            get
            {
                var obj = (KeyValuePair<DyObject, DyObject>)enumerator.Current;
                return new DyTuple(rtx.Tuple, new DyObject[] {
                        new DyLabel(rtx.Label, "key", obj.Key),
                        new DyLabel(rtx.Label, "value", obj.Value)
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
