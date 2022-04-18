using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Dyalect
{
    public sealed class FastList<T> : IEnumerable<T>
    {
        internal static readonly FastList<T> Empty = new();
        private T[] array;
        private const int DEFAULT_SIZE = 4;
        private int size;
        private int initialSize;

        public FastList() : this(DEFAULT_SIZE) { }

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

        public FastList<T> Clone()
        {
            var list = new FastList<T>
            {
                array = (T[])array.Clone(),
                size = size,
                initialSize = initialSize
            };
            return list;
        }

        public bool Contains(T val)
        {
            for (var i = 0; i < size; i++)
                if (array[i]!.Equals(val))
                    return true;

            return false;
        }

        public T[] ToArray()
        {
            var arr = new T[size];
            Array.Copy(array, arr, size);
            return arr;
        }

        public int IndexOf(T elem) => Array.IndexOf(array, elem);

        internal T[] GetRawArray() => array;

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

        internal void AddRange(FastList<T> arr, int start, int end)
        {
            for (var i = start; i < end; i++)
                Add(arr[i]);
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
                var dest = new T[array.Length == 0 ? DEFAULT_SIZE : array.Length * 2];
                Array.Copy(array, 0, dest, 0, size);
                array = dest;
            }

            array[size++] = val;
        }

        internal bool RemoveAt(int index)
        {
            if (index >= 0 && index < size)
            {
                size--;

                if (index < size)
                    Array.Copy(array, index + 1, array, index, size - index);

                array[size] = default!;
                return true;
            }

            return false;
        }

        internal bool Remove(T val) => RemoveAt(Array.IndexOf(array, val));

        internal void Trim()
        {
            if (array.Length > size)
            {
                var newArr = new T[size];
                Array.Copy(array, newArr, size);
                array = newArr;
            }
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
}