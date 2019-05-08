using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public sealed class ExecutionContext
    {
        internal int AUX;

        internal ExecutionContext(CallStack callStack, UnitComposition composition)
        {
            CallStack = callStack;
            Composition = composition;
            Units = new DyObject[Composition.Units.Count][];
        }

        internal DyObject[][] Units { get; }

        internal FastList<DyTypeInfo> Types => Composition.Types;

        public UnitComposition Composition { get; }

        public bool HasErrors => Error != null;

        internal CallStack CallStack { get; }

        internal DyError Error { get; set; }

        internal Stack<int> Sections { get; set; }

        internal Stack<DyObject[]> Locals { get; } = new Stack<DyObject[]>();

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
