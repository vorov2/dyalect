using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyMapEnumerable : IEnumerable<DyObject>
    {
        private readonly DyMap obj;

        public DyMapEnumerable(DyMap obj) => this.obj = obj;

        public IEnumerator<DyObject> GetEnumerator() => new DyMapEnumerator(obj);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
