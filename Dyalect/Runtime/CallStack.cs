using Dyalect.Runtime.Types;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    internal sealed class CallStack : IEnumerable<Caller>
    {
        private const int DEFAULT_SIZE = 4;
        private Caller[] array;
        private readonly int initialSize;

        public CallStack() : this(DEFAULT_SIZE)
        {

        }

        public CallStack(int size)
        {
            this.initialSize = size;
            array = new Caller[size];
        }

        public IEnumerator<Caller> GetEnumerator()
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
            array = new Caller[initialSize];
        }

        public Caller Pop()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException();

            return array[--Count];
        }

        public bool PopLast()
        {
            array[--Count] = null;
            return true;
        }

        public Caller Peek()
        {
            return array[Count - 1];
        }

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

        public CallStack Clone() => (CallStack)MemberwiseClone();

        public int Count;

        public Caller this[int index]
        {
            get { return array[index]; }
            set { array[index] = value; }
        }
    }

    internal sealed class Caller
    {
        public static readonly Caller Root = new Caller();
        public static readonly Caller External = new Caller();

        private Caller() { }

        public Caller(DyNativeFunction function, int offset, EvalStack evalStack, DyObject[] locals)
        {
            Function = function;
            Offset = offset;
            EvalStack = evalStack;
            Locals = locals;
        }

        public Stack<DyObject> Autos;
        public readonly DyObject[] Locals;
        public readonly EvalStack EvalStack;
        public readonly int Offset;
        public readonly DyNativeFunction Function;
    }

    internal readonly struct CatchMark
    {
        public CatchMark(int offset, int stackOffset)
        {
            Offset = offset;
            StackOffset = stackOffset;
        }

        public readonly int Offset;

        public readonly int StackOffset;
    }
}