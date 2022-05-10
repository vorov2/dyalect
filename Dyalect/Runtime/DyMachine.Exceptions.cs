using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
namespace Dyalect.Runtime;

partial class DyMachine
{
    private static bool TryCall(ExecutionContext ctx, int offset, ref DyObject? arg1, ref DyObject? arg2,
        ref DyNativeFunction function, ref DyObject[] locals, ref EvalStack evalStack)
    {
        if (ReferenceEquals(ctx.Error, DyVariant.Eta))
        {
            ctx.CallStack.Push(new Caller(function, offset, evalStack, locals));
            function = (DyNativeFunction)ctx.EtaFunction!;
            ctx.EtaFunction = null;
            ctx.Error = null;
            locals = function.CreateLocals(ctx);
            if (arg1 is not null) locals[0] = arg1;
            if (arg2 is not null) locals[1] = arg2;
            arg1 = null;
            arg2 = null;
            return true;
        }

        return false;
    }

    private static int ThrowIf(ExecutionContext ctx, int offset, ref DyNativeFunction function, ref DyObject[] locals, ref EvalStack evalStack)
    {
        var err = ctx.Error!;

        if (FindCatch(ctx, ref function, ref locals, ref evalStack, out var address))
        {
            ctx.ErrorDump = Dump(ctx, offset, function);
            return address;
        }
        else
        {
            var dump = Dump(ctx, offset, function);
            var cs = ctx.Trace ?? new DyDebugger(ctx.RuntimeContext.Composition).BuildCallStack(dump);
            ctx.Error = null;
            ctx.ErrorDump = null;
            ctx.Trace = null;
            throw new DyCodeException(err, cs, null);
        }
    }

    private static bool FindCatch(ExecutionContext ctx, ref DyNativeFunction function, ref DyObject[] locals, ref EvalStack evalStack, out int offset)
    {
        CatchMark mark = default;
        Stack<CatchMark> cm;
        var idx = 1;
        offset = 0;

        while (ctx.CatchMarks.TryPeek(idx++, out cm))
        {
            if (cm is not null && cm.Count > 0)
            {
                mark = cm.Peek();
                break;
            }
        }

        if (mark.Offset == 0)
            return false;

        Caller? cp = null;

        while (ctx.CallStack.Count > mark.StackOffset)
        {
            cp = ctx.CallStack.Pop();

            //It means that this function was called from an external
            //context and we have to terminate our search
            if (ReferenceEquals(cp, Caller.External))
                return false;
        }

        cm.Pop();

        if (cp is not null)
        {
            function = cp.Function;
            locals = cp.Locals;
            evalStack = cp.EvalStack;
        }

        offset = mark.Offset;
        return true;
    }

    private static Stack<StackPoint> Dump(ExecutionContext ctx, int offset, DyNativeFunction function)
    {
        var dump = ctx.ErrorDump;

        if (dump is null)
        {
            var callStack = ctx.CallStack.Clone();
            dump = new Stack<StackPoint>();
            var sp = StackPoint.Empty;

            for (var i = 0; i < callStack.Count; i++)
            {
                var cm = callStack[i];

                if (ReferenceEquals(cm, Caller.Root))
                    continue;

                if (ReferenceEquals(cm, Caller.External))
                    sp = StackPoint.External;
                else
                    sp = new(cm.Offset, cm.Function.UnitId);

                dump.Push(sp);
            }

            if (sp.IsEmpty || sp.Offset != offset || sp.UnitId != function.UnitId)
                dump.Push(new(offset, function.UnitId));
        }

        return dump;
    }

    public static IEnumerable<RuntimeVar> DumpVariables(RuntimeContext rtx)
    {
        foreach (var v in rtx.Composition.Units[0].GlobalScope!.EnumerateVars())
            yield return new(v.Key, rtx.Units[0][v.Value.Address]);
    }

    private static (DyVariant err, CallStackTrace? trace) GetErrorInformation(DyFunction func, Exception ex)
    {
        if (ex is DyCodeException err)
            return (err.Error, err.CallTrace);

        if (ex.InnerException is not null)
            return GetErrorInformation(func, ex.InnerException);

        var functionName = func.Self is null ? func.FunctionName
            : $"{func.Self.TypeName}.{func.FunctionName}";
        return (new(DyError.ExternalFunctionFailure, functionName, ex.Message), null);
    }
}
