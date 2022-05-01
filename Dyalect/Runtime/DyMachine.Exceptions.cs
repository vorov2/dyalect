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
        var dump = ctx.ErrorDump;
        var moduleHandle = function.UnitId;

        if (dump is null)
        {
            dump = Dump(ctx.CallStack.Clone());
            dump.Push(new(offset, moduleHandle));
        }

        int jumper;

        if ((jumper = FindCatch(ctx, ref function, ref locals, ref evalStack)) > -1)
        {
            ctx.ErrorDump = dump;
            return jumper;
        }
        else
        {
            ctx.Error = null;
            ctx.ErrorDump = null;
            var deb = new DyDebugger(ctx.RuntimeContext.Composition);
            var cs = deb.BuildCallStack(dump);
            throw new DyCodeException(err, cs, null);
        }
    }

    private static int FindCatch(ExecutionContext ctx, ref DyNativeFunction function, ref DyObject[] locals, ref EvalStack evalStack)
    {
        CatchMark mark = default;
        Stack<CatchMark> cm;
        var idx = 1;

        while (ctx.CatchMarks.TryPeek(idx++, out cm))
        {
            if (cm is not null && cm.Count > 0)
            {
                mark = cm.Peek();
                break;
            }
        }

        if (mark.Offset == 0)
            return -1;

        Caller? cp = null;

        while (ctx.CallStack.Count > mark.StackOffset)
        {
            cp = ctx.CallStack.Pop();

            //It means that this function was called from an external
            //context and we have to terminate our search
            if (ReferenceEquals(cp, Caller.External))
                return -1;
        }

        cm.Pop();

        if (cp is not null)
        {
            function = cp.Function;
            locals = cp.Locals;
            evalStack = cp.EvalStack;
        }

        return mark.Offset;
    }

    private static Stack<StackPoint> Dump(CallStack callStack)
    {
        var st = new Stack<StackPoint>();

        for (var i = 0; i < callStack.Count; i++)
        {
            var cm = callStack[i];

            if (ReferenceEquals(cm, Caller.Root))
                continue;

            if (ReferenceEquals(cm, Caller.External))
                st.Push(StackPoint.External);
            else
                st.Push(new(cm.Offset, cm.Function.UnitId));
        }

        return st;
    }

    public static IEnumerable<RuntimeVar> DumpVariables(ExecutionContext ctx)
    {
        foreach (var v in ctx.RuntimeContext.Composition.Units[0].GlobalScope!.EnumerateVars())
            yield return new(v.Key, ctx.RuntimeContext.Units[0][v.Value.Address]);
    }

    private static DyVariant GetErrorInformation(DyFunction func, Exception ex)
    {
        if (ex is DyCodeException err)
            return err.Error;

        if (ex.InnerException is not null)
            return GetErrorInformation(func, ex.InnerException);

        var functionName = func.Self is null ? func.FunctionName
            : $"{func.Self.TypeName}.{func.FunctionName}";
        return new(DyErrorCode.ExternalFunctionFailure, functionName, ex.Message);
    }
}
