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

        public ExecutionContext Clone() => new(CallStack, RuntimeContext);

        internal CallStack CallStack { get; }

        internal SectionStack CatchMarks { get; }
        
        internal DyError Error { get; set; }

        internal Stack<int> Sections { get; set; }

        internal Stack<ArgContainer> Arguments { get; } = new(4);

        public DyError GetError() => Error;

        internal void ThrowIf()
        {
            if (Error is not null)
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
