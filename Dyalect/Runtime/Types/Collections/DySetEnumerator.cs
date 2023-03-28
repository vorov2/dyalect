using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types;

internal sealed class DySetEnumerator : IEnumerator<DyObject>
{
    private readonly DySet obj;
    private readonly IEnumerator<DyObject> enumerator;
    private readonly int version;

    public DySetEnumerator(DySet obj)
    {
        this.obj = obj;
        version = obj.Version;
        enumerator = obj.Set.GetEnumerator();
    }

    public DyObject Current => enumerator.Current;

    object IEnumerator.Current => Current;

    public void Dispose() { }

    public bool MoveNext() =>
        version != obj.Version ? throw new IterationException() : enumerator.MoveNext();

    public void Reset() => enumerator.Reset();
}
