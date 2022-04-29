using Dyalect.Compiler;
using System.Collections.Generic;
namespace Dyalect.Debug;

public sealed class DyDebugger
{
    private const string DefaultName = "<func>";
    private const string Unknown = "<unknown>";
    private const string Global = "<global>";

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

        if (callChain is null || callChain.Count == 0)
            return retval;

        do
        {
            var mem = callChain.Pop();

            if (mem.IsExternal)
            {
                frames.Add(CallFrame.External);
                continue;
            }

            var offset = mem.BreakAddress - 1;
            var unit = Composition.Units[mem.UnitHandle];

            if (unit.Symbols is null)
            {
                frames.Add(new(
                    moduleName: unit.FileName,
                    codeBlockName: Unknown,
                    offset: offset,
                    lineSym: new(offset)));
                continue;
            }

            var funSym = unit.Symbols.FindFunSym(offset);
            var line = unit.Symbols.FindLineSym(offset);
            string? codeBlockName = null;

            if (funSym != null)
            {
                codeBlockName = funSym.Name ?? DefaultName;

                if (funSym.TypeName is not null)
                    codeBlockName = funSym.TypeName + "." + codeBlockName;

                if (funSym.Parameters is not null)
                    codeBlockName += "(" + string.Join(",", funSym.Parameters) + ")";
                else
                    codeBlockName += "(...)";
            }

            frames.Add(new(
                moduleName: unit.FileName,
                codeBlockName: codeBlockName ?? Global,
                offset: offset,
                lineSym: line ?? new LineSym(offset)));
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

    internal List<Breakpoint> Breakpoints { get; }

    internal UnitComposition Composition { get; }
}
