using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    internal sealed class CallPoint
    {
        internal CallPoint(int returnAddress, EvalStack stack, DyObject[] locals, DyFunction func)
        {
            ReturnAddress = returnAddress;
            Locals = locals;
            Stack = stack;
            Function = func;
        }

        public int ReturnAddress;

        public readonly DyObject[] Locals;

        public readonly DyFunction Function;

        public readonly EvalStack Stack;
    }
}
