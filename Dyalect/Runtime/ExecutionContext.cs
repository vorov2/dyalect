using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public sealed class ExecutionContext
    {
        internal int AUX;

        internal static readonly ExecutionContext Default = new ExecutionContext(new CallStack(), new UnitComposition(new List<Unit>()));

        internal ExecutionContext(CallStack callStack, UnitComposition composition)
        {
            CallStack = callStack;
            CatchMarks = new Stack<CatchMark>();
            Composition = composition;
            Units = new DyObject[Composition.Units.Count][];
        }

        internal DyObject[][] Units { get; }

        internal List<DyTypeInfo> Types => Composition.Types;

        public UnitComposition Composition { get; }

        public bool HasErrors => Error != null;

        internal CallStack CallStack { get; }

        internal Stack<CatchMark> CatchMarks { get; }

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
