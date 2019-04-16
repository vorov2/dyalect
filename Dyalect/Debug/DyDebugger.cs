using Dyalect.Compiler;
using System;
using System.Collections.Generic;

namespace Dyalect.Debug
{
    public sealed class DyDebugger
    {
        private const string FUNC = "<func>";

        public DyDebugger(UnitComposition asm, int moduleHandle)
        {
            Composition = asm;
            ModuleHandle = moduleHandle;

            if (CodeUnit.Symbols.Lines.Count > 0)
                Breakpoints = new List<Breakpoint>();
        }

        //TODO: Debug Execution
        //public ExecutionResult Execute(IDebugObserver observer)
        //{
        //    var monitor = new DebuggerMonitor(new Debugger(Composition, ModuleHandle), observer);
        //    var vm = new DysMachine(Composition, monitor);
        //    return vm.Execute();
        //}

        public CallStackTrace BuildCallStack(int currentOffset, Stack<int> callChain)
        {
            var syms = new DebugReader(CodeUnit.Symbols);
            var frames = new List<CallFrame>();
            var lp = syms.FindLineSym(currentOffset);
            var retval = new CallStackTrace(
                    CodeUnit.FileName,
                    lp != null ? lp.Line : 0,
                    lp != null ? lp.Column : 0,
                    frames
                );

            if (callChain == null || callChain.Count == 0)
                return retval;

            do
            {
                var mem = callChain.Pop() - 1;
                syms = new DebugReader(CodeUnit.Symbols);
                var funSym = syms.FindFunSym(mem);
                var line = syms != null ? syms.FindLineSym(mem) : null;
                frames.Add(new CallFrame(funSym == null,
                    CodeUnit.FileName,
                    funSym != null ? (funSym?.Name ?? FUNC) : "global",
                    mem, line));
            }
            while (callChain.Count > 0);

            return retval;
        }

        public bool AddBreakpoint(Breakpoint bp)
        {
            if (Breakpoints == null || !ResolveBreakpoint(bp))
                return false;

            Breakpoints.Add(bp);
            return true;
        }

        public bool ResolveBreakpoint(Breakpoint bp)
        {
            var dr = new DebugReader(CodeUnit.Symbols);
            var sym = dr.FindClosestLineSym(bp.Line, bp.Column);

            if (sym == null)
                return false;

            bp.Offset = sym.Offset;
            return true;
        }

        internal List<Breakpoint> Breakpoints { get; }

        internal UnitComposition Composition { get; }

        internal int ModuleHandle { get; }

        internal Unit CodeUnit => Composition.Units[ModuleHandle];
    }
}
