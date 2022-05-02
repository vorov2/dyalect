using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect.Runtime;

internal sealed class CallStack : IEnumerable<Caller>
{
    private const int DefaultSize = 6;

    private Caller[] array;
    private readonly int initialSize;

    public int Count { get; private set; }

    public Caller this[int index]
    {
        get => array[index];
        set => array[index] = value;
    }

    public CallStack() : this(DefaultSize) { }

    private CallStack(int size)
    {
        this.initialSize = size;
        array = new Caller[size];
    }

    public IEnumerator<Caller> GetEnumerator() => array.Take(Count).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public CallStack Clone() => (CallStack)MemberwiseClone();

    public void Clear() => (Count, array) = (0, new Caller[initialSize]);

    public Caller Pop() => Count == 0 ? throw new IndexOutOfRangeException() : array[--Count];

    public bool PopLast()
    {
        array[--Count] = null!;
        return true;
    }

    public Caller Peek() => array[Count - 1];

    public void Push(Caller val)
    {
        if (Count == array.Length)
        {
            var dest = new Caller[array.Length * 2];

            for (var i = 0; i < Count; i++)
                dest[i] = array[i];

            array = dest;
        }

        array[Count++] = val;
    }
}
