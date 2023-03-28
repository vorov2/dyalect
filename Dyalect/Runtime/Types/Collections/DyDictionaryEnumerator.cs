using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types;

internal sealed class DyDictionaryEnumerator : IEnumerator<DyObject>
{
    private readonly DyDictionary obj;
    private readonly IEnumerator enumerator;
    private readonly int version;

    public DyDictionaryEnumerator(DyDictionary obj)
    {
        this.obj = obj;
        version = obj.Version;
        enumerator = obj.Dictionary.GetEnumerator();
    }

    public DyObject Current
    {
        get
        {
            var obj = (KeyValuePair<DyObject, DyObject>)enumerator.Current;
            return new DyTuple(new DyLabel[] {
                new("key", obj.Key),
                new("value", obj.Value)
            });
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose() { }

    public bool MoveNext() =>
        version != obj.Version ? throw new IterationException() : enumerator.MoveNext();

    public void Reset() => enumerator.Reset();
}
