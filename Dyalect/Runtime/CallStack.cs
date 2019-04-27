using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    internal sealed class CallStack : IEnumerable<CallPoint>
    {
        private const int DEFAULT_SIZE = 4;
        private CallPoint[] array;
        private int initialSize;

        public CallStack() : this(DEFAULT_SIZE)
        {

        }

        public CallStack(int size)
        {
            this.initialSize = size;
            array = new CallPoint[size];
        }

        public IEnumerator<CallPoint> GetEnumerator()
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
            array = new CallPoint[initialSize];
        }

        public ref CallPoint Pop()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException();

            return ref array[--Count];
        }

        public ref CallPoint Peek()
        {
            return ref array[Count - 1];
        }

        public void Push(CallPoint val)
        {
            if (Count == array.Length)
            {
                var dest = new CallPoint[array.Length * 2];

                for (var i = 0; i < Count; i++)
                    dest[i] = array[i];

                array = dest;
            }

            array[Count++] = val;
        }

        public int Count;

        public CallPoint this[int index]
        {
            get { return array[index]; }
            set { array[index] = value; }
        }
    }
}