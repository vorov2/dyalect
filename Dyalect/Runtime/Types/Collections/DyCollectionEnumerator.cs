using System.Collections;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

internal sealed class DyCollectionEnumerable : IEnumerable<DyObject>
{
    private readonly DyObject[] arr;
    private readonly int count;
    private readonly DyCollection obj;
    private readonly int start;

    public DyCollectionEnumerable(DyObject[] arr, int start, int count, DyCollection obj)
    {
        this.arr = arr;
        this.start = start;
        this.count = count;
        this.obj = obj;
    }

    public IEnumerator<DyObject> GetEnumerator() => new DyCollectionEnumerator(arr, start, count, obj);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


internal sealed class DyCollectionEnumerator : IEnumerator<DyObject>
{
    private readonly DyObject[] arr;
    private readonly int count;
    private readonly DyCollection obj;
    private readonly int version;
    private readonly int start;
    private int index = -1;

    public DyCollectionEnumerator(DyObject[] arr, int start, int count, DyCollection obj)
    {
        this.arr = arr;
        this.start = start;
        this.count = count;
        this.obj = obj;
        version = obj.Version;
    }

    public DyObject Current => arr[index + start];

    object IEnumerator.Current => Current;

    public void Dispose() { }

    public bool MoveNext() =>
        version != obj.Version ? throw new IterationException() : ++index < count;

    public void Reset() => index = -1;
}
