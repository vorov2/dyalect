using System.Collections;
using System.Collections.Generic;
using CatchMarks = System.Collections.Generic.Stack<Dyalect.Runtime.CatchMark>;

namespace Dyalect.Runtime;

internal sealed class SectionStack : IEnumerable<CatchMarks>
{
    private const int DefaultSize = 4;
    private CatchMarks[] array;
    private readonly int initialSize;

    public int Count { get; private set; }

    public SectionStack() : this(DefaultSize) { }

    public SectionStack(int size)
    {
        initialSize = size;
        array = new CatchMarks[size];
    }

    public IEnumerator<CatchMarks> GetEnumerator()
    {
        var c = Count;

        while (c > 0)
            yield return array[--c];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear()
    {
        Count = 0;
        array = new CatchMarks[initialSize];
    }

    public CatchMarks Pop() => Count == 0 ? throw new IndexOutOfRangeException() : array[--Count];

    public CatchMarks Peek() => array[Count - 1];

    public bool TryPeek(int i, out CatchMarks val)
    {
        if (Count - i < 0)
        {
            val = default!;
            return false;
        }

        val = array[Count - i];
        return true;
    }

    public void Push(CatchMarks val)
    {
        if (Count == array.Length)
        {
            var dest = new CatchMarks[array.Length * 2];

            for (var i = 0; i < Count; i++)
                dest[i] = array[i];

            array = dest;
        }

        array[Count++] = val;
    }

    public void Replace(CatchMarks val) => array[Count - 1] = val;

    public CatchMarks this[int index]
    {
        get => array[index];
        set => array[index] = value;
    }
}
