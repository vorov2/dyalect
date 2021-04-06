using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    using CatchMarks = Stack<CatchMark>;

    internal sealed class SectionStack : IEnumerable<CatchMarks>
    {
        private const int DEFAULT_SIZE = 4;
        private CatchMarks[] array;
        private readonly int initialSize;
        public int Count;

        public SectionStack() : this(DEFAULT_SIZE)
        {

        }

        public SectionStack(int size)
        {
            this.initialSize = size;
            array = new CatchMarks[size];
        }

        public IEnumerator<CatchMarks> GetEnumerator()
        {
            var c = Count;

            while (c > 0)
                yield return array[--c];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            Count = 0;
            array = new CatchMarks[initialSize];
        }

        public CatchMarks Pop()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException();

            return array[--Count];
        }

        public CatchMarks Peek()
        {
            return array[Count - 1];
        }

        public bool TryPeek(int i, out CatchMarks val)
        {
            if (Count - i < 0)
            {
                val = default;
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

        public void Replace(CatchMarks val)
        {
            array[Count - 1] = val;
        }

        public CatchMarks this[int index]
        {
            get { return array[index]; }
            set { array[index] = value; }
        }
    }
}
