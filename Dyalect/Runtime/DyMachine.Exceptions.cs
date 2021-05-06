using Dyalect.Debug;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    partial class DyMachine
    {
        private static bool ProcessError(ExecutionContext ctx, int offset, ref DyNativeFunction function,
            ref DyObject[] locals, ref EvalStack evalStack, ref int jumper)
        {
            var err = ctx.Error!;
            jumper = ThrowIf(err, offset, function.UnitId, ref function, ref locals, ref evalStack, ctx);
            return jumper > -1;
        }

        private static int ThrowIf(DyError err, int offset, int moduleHandle, ref DyNativeFunction function,
            ref DyObject[] locals, ref EvalStack evalStack, ExecutionContext ctx)
        {
            Stack<StackPoint> dump;

            if (err.Dump is null)
            {
                dump = Dump(ctx.CallStack.Clone());
                dump.Push(new(offset, moduleHandle));
            }
            else
                dump = err.Dump;

            int jumper;

            if ((jumper = FindCatch(ctx, ref function, ref locals, ref evalStack)) > -1)
            {
                ctx.Error!.Dump = dump;
                return jumper;
            }
            else
            {
                ctx.Error = null;
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
                    st.Push(new(external: true));
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

        private static DyCodeException? GetCodeException(Exception ex)
        {
            if (ex is DyCodeException dy)
                return dy;
            
            if (ex.InnerException is not null)
                return GetCodeException(ex.InnerException);

            return null;
        }
    }
}
