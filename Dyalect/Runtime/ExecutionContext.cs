using System.Collections.Generic;
using Dyalect.Compiler;
using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public sealed class ExecutionContext
    {
        internal ExecutionContext(CallStack callStack, UnitComposition asm)
        {
            CallStack = callStack;
            Assembly = asm;
        }

        public UnitComposition Assembly { get; }

        internal CallStack CallStack { get; }

        internal DyError Error { get; set; }

        internal Stack<int> Sections { get; set; }

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
}
