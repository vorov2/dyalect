using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    //Used to create MultiPartEnumerator
    internal sealed class MultiPartEnumerable : IEnumerable<DyObject>
    {
        private readonly DyObject[] iterators;
        private readonly ExecutionContext ctx;

        public MultiPartEnumerable(ExecutionContext ctx, params DyObject[] iterators) =>
            (this.ctx, this.iterators) = (ctx, iterators);

        public IEnumerator<DyObject> GetEnumerator() => new MultiPartEnumerator(ctx, iterators);

        IEnumerator IEnumerable.GetEnumerator() => new MultiPartEnumerator(ctx, iterators);
    }
}
