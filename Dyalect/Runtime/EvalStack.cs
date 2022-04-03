using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public sealed class EvalStack
    {
        private readonly DyObject[] array;
        private int size;

        public EvalStack(int size) => array = new DyObject[size];

        internal void Dup() => array[size++] = array[size - 2];

        internal DyObject Pop()
        {
            var ret = array[--size];
            array[size] = null!;
            return ret;
        }

        internal void PopVoid() => array[--size] = null!;

        internal void Clear()
        {
            while (size > 0)
                array[--size] = null!;
        }

        internal DyObject Peek() => array[size - 1];

        internal DyObject Peek(int n) => array[size - n];

        internal void Push(DyObject val) => array[size++] = val;

        internal void Replace(DyObject val) => array[size - 1] = val;

        internal void Push(bool val) => array[size++] = val ? DyBool.True : DyBool.False;

        internal void Replace(bool val) => array[size - 1] = val ? DyBool.True : DyBool.False;

        internal int Size => size;
    }
}