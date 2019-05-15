using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    internal sealed class CallStack : IEnumerable<long>
    {
        private const int DEFAULT_SIZE = 4;
        private long[] array;
        private int initialSize;

        public CallStack() : this(DEFAULT_SIZE)
        {

        }

        public CallStack(int size)
        {
            this.initialSize = size;
            array = new long[size];
        }

        public IEnumerator<long> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            Count = 0;
            array = new long[initialSize];
        }

        public ref long Pop()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException();

            return ref array[--Count];
        }

        public ref long Peek()
        {
            return ref array[Count - 1];
        }

        public void Push(long val)
        {
            if (Count == array.Length)
            {
                var dest = new long[array.Length * 2];

                for (var i = 0; i < Count; i++)
                    dest[i] = array[i];

                array = dest;
            }

            array[Count++] = val;
        }

        public void Dup()
        {
            if (Count > 0)
                Push(Peek());
            else
                Push((long)0 | (long)0 << 32);
        }

        public int Count;

        public long this[int index]
        {
            get { return array[index]; }
            set { array[index] = value; }
        }
    }
}