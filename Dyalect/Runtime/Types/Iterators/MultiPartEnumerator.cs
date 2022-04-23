using System.Collections;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

//Used to implement "concat" method when several iterators are combined in one
internal sealed class MultiPartEnumerator : IEnumerator<DyObject>
{
    private readonly DyObject[] iterators;
    private int nextIterator = 0;
    private IEnumerator<DyObject>? current;
    private readonly ExecutionContext ctx;

    public MultiPartEnumerator(ExecutionContext ctx, params DyObject[] iterators) =>
        (this.ctx, this.iterators) = (ctx, iterators);

    public DyObject Current => current!.Current;

    object IEnumerator.Current => current!.Current;

    public void Dispose() { }

    public bool MoveNext()
    {
        if (current is null || !current.MoveNext())
        {
            if (iterators.Length > nextIterator)
            {
                var it = DyIterator.ToEnumerable(ctx, iterators[nextIterator]);
                nextIterator++;
                ctx.ThrowIf();
                current = it.GetEnumerator();
                return current.MoveNext();
            }
            else
                return false;
        }
        else
            return true;
    }

    public void Reset()
    {
        current = null;
        nextIterator = 0;
    }
}
