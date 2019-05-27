﻿using Dyalect.Runtime.Types;
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

        public CallPoint Pop()
        {
            if (Count == 0)
                throw new IndexOutOfRangeException();

            return array[--Count];
        }

        public CallPoint Peek()
        {
            return array[Count - 1];
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

        public CallStack Clone() => (CallStack)MemberwiseClone();

        public int Count;

        public CallPoint this[int index]
        {
            get { return array[index]; }
            set { array[index] = value; }
        }
    }

    internal sealed class CallPoint
    {
        public static readonly CallPoint External = new CallPoint { Locals = Statics.EmptyDyObjects, Function = DyMachine.Global };

        public DyObject[] Locals;
        public EvalStack EvalStack;
        public int Offset;
        public CatchMark CatchMark;
        public DyNativeFunction Function;
    }

    internal sealed class CatchMark
    {
        public CatchMark(int offset)
        {
            Offset = offset;
        }

        public readonly int Offset;
        public CatchMark Previous;
    }
}