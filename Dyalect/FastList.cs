using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Dyalect;

public sealed class FastList<T> : IEnumerable<T>
{
    private const int DefaultSize = 6;

    internal static readonly FastList<T> Empty = new();
    private T[] array;
    private int size;
    private int initialSize;

    public FastList() : this(DefaultSize) { }

    internal FastList(int size)
    {
        initialSize = size;
        array = new T[size];
    }

    internal FastList(FastList<T> list)
    {
        this.initialSize = list.initialSize;
        array = (T[])list.array.Clone();
        size = list.size;
    }

    internal FastList(T[] arr)
    {
        initialSize = arr.Length;
        size = arr.Length;
        array = arr;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < size; i++)
            yield return array[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public FastList<T> Clone() =>
        new()
        {
            array = (T[])array.Clone(),
            size = size,
            initialSize = initialSize
        };

    public T[] ToArray()
    {
        var arr = new T[size];
        Array.Copy(array, arr, size);
        return arr;
    }

    internal T[] UnsafeGetArray() => array;

    internal void Clear()
    {
        size = 0;
        array = new T[initialSize];
    }

    internal void AddRange(IEnumerable<T> seq)
    {
        foreach (var s in seq)
            Add(s);
    }

    internal void AddRange(T[] arr, int start, int end)
    {
        for (var i = start; i < end; i++)
            Add(arr[i]);
    }

    internal void Add(T val)
    {
        if (size == array.Length)
        {
            var dest = new T[array.Length == 0 ? DefaultSize : array.Length * 2];
            Array.Copy(array, 0, dest, 0, size);
            array = dest;
        }

        array[size++] = val;
    }

    public int Count => size;

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => array[index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set => array[index] = value;
    }
}
