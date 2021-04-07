using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public sealed class ExecutionContext
    {
        internal int AUX;

        internal ExecutionContext(CallStack callStack, RuntimeContext rtx)
        {
            CallStack = callStack;
            CatchMarks = new();
            RuntimeContext = rtx;
        }

        public RuntimeContext RuntimeContext { get; }

        public bool HasErrors => Error != null;

        internal CallStack CallStack { get; }

        internal SectionStack CatchMarks { get; }

        internal DyError OldError { get; set; }
        
        internal DyError Error { get; set; }

        internal Stack<int> Sections { get; set; }

        internal Stack<ArgContainer> Arguments { get; } = new Stack<ArgContainer>(4);

        public DyError GetError() => Error;

        internal void ThrowIf()
        {
            if (Error != null)
            {
                var err = Error;
                Error = null;
                throw new DyRuntimeException(err.GetDescription());
            }
        }

    }

    internal struct ArgContainer
    {
        public DyObject[] Locals;
        public FastList<DyObject> VarArgs;
        public int VarArgsIndex;
    }
}
