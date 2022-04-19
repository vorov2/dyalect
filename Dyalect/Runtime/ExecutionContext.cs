using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public class ExecutionContext
    {
        internal int RgDI; //RgDI register
        internal int RgFI; //RgFI register
        internal int CallCnt; //Call counter

        public static ExecutionContext External { get; } = new ExternalExecutionContext();

        private sealed class ExternalExecutionContext : ExecutionContext
        {
            internal ExternalExecutionContext() : base(null!, null!) { }

            internal override DyVariant? Error
            {
                get => base.Error;
                set
                {
                    base.Error = value;
                    ThrowRuntimeException();
                }
            }
        }

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

        private DyVariant? _error;
        internal virtual DyVariant? Error
        {
            get => _error;
            set
            {
                if (_error is null || value is null)
                    _error = value;
            }
        }

        internal Stack<StackPoint>? ErrorDump { get; set; }

        internal Stack<int>? Sections { get; set; }

        internal int UnitId;
        internal int CallerUnitId;

        public DyVariant? GetError() => Error;

        public void ThrowIf()
        {
            if (Error is not null)
            {
                var err = Error;
                Error = null;
                throw new BreakException(err);
            }
        }

        public void ThrowRuntimeException()
        {
            if (Error is not null)
            {
                var err = Error;
                Error = null;
                throw new DyRuntimeException(ErrorGenerators.GetErrorDescription(err));
            }
        }

        #region ArgContainer
        private int count;
        private readonly List<ArgContainer> containers = new(2);

        internal sealed class ArgContainer
        {
            public DyObject[] Locals = null!;
            public DyObject[]? VarArgs;
            public int VarArgsSize;
            public int VarArgsIndex;
        }

        internal ArgContainer PushArguments(DyObject[] locals, int varArgsIndex, DyObject[]? varArgs = null)
        {
            if (containers.Count <= count)
                containers.Add(new());

            var ret = containers[count++];
            ret.Locals = locals;
            ret.VarArgsIndex = varArgsIndex;
            ret.VarArgs = varArgs;
            ret.VarArgsSize = 0;
            return ret;
        }

        internal ArgContainer PopArguments() => containers[--count];

        internal ArgContainer PeekArguments() => containers[count - 1];
        #endregion
    }
}
