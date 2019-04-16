using Dyalect.Compiler;
using System;
using System.Collections.Generic;

namespace Dyalect.Debug
{
    public sealed class DyDebugger
    {
        private const string FUNC = "<func>";

        public DyDebugger(UnitComposition asm)
        {
            Composition = asm;

            //if (CodeUnit.Symbols.Lines.Count > 0)
            //    Breakpoints = new List<Breakpoint>();
        }

        //TODO: Debug Execution
        //public ExecutionResult Execute(IDebugObserver observer)
        //{
        //    var monitor = new DebuggerMonitor(new Debugger(Composition, ModuleHandle), observer);
        //    var vm = new DysMachine(Composition, monitor);
        //    return vm.Execute();
        //}

        public CallStackTrace BuildCallStack(Stack<StackPoint> callChain)
        {
            var frames = new List<CallFrame>();
            var retval = new CallStackTrace(frames);

            if (callChain == null || callChain.Count == 0)
                return retval;

            do
            {
                var mem = callChain.Pop();
                var offset = mem.BreakAddress - 1;
                var unit = Composition.Units[mem.UnitHandle];
                var syms = new DebugReader(unit.Symbols);
                var funSym = syms.FindFunSym(offset);
                var line = syms?.FindLineSym(offset);
                string codeBlockName = null;

                if (funSym != null)
                {
                    codeBlockName = funSym.Name ?? FUNC;

                    if (funSym.Parameters != null)
                        codeBlockName += "(" + string.Join(",", funSym.Parameters) + ")";
                    else
                        codeBlockName += "(...)";
                }

                frames.Add(new CallFrame(
                    moduleName: unit.FileName,
                    codeBlockName: codeBlockName,
                    offset: offset, 
                    lineSym: line));
            }
            while (callChain.Count > 0);

            return retval;
        }

        //public bool AddBreakpoint(Breakpoint bp)
        //{
        //    if (Breakpoints == null || !ResolveBreakpoint(bp))
        //        return false;

        //    Breakpoints.Add(bp);
        //    return true;
        //}

        //public bool ResolveBreakpoint(Breakpoint bp)
        //{
        //    var dr = new DebugReader(CodeUnit.Symbols);
        //    var sym = dr.FindClosestLineSym(bp.Line, bp.Column);

        //    if (sym == null)
        //        return false;

        //    bp.Offset = sym.Offset;
        //    return true;
        //}

        //internal List<Breakpoint> Breakpoints { get; }

        internal UnitComposition Composition { get; }
    }
}
