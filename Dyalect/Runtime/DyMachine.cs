using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Linker;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public static class DyMachine
    {
        private static readonly DyNativeFunction global = new DyNativeFunction(0, 0, 0, FastList<DyObject[]>.Empty, StandardType.Function);

        public static ExecutionContext CreateExecutionContext(UnitComposition composition)
        {
            return new ExecutionContext(new CallStack(), composition);
        }

        public static ExecutionResult Execute(ExecutionContext ctx)
        {
            ExecutionResult retval = null;

#if !DEBUG
            Exception eex = null;

            var th = new System.Threading.Thread(() =>
            {
                try
                {
                    var res = ExecuteModule(0, ctx);
                    retval = ExecutionResult.Fetch(0, res, ctx);
                }
                catch (Exception ex)
                {
                    eex = ex;
                }
            }, 8 * 1024 * 1024);
            th.Start();
            th.Join();

            if (eex != null)
                throw eex;
#else
            var res = ExecuteModule(0, ctx);
            retval = ExecutionResult.Fetch(0, res, ctx);
#endif
            return retval;
        }

        private static DyObject ExecuteModule(int unitId, ExecutionContext ctx)
        {
            var unit = ctx.Composition.Units[unitId];

            if (unit.Layouts.Count == 0)
            {
                var foreign = (ForeignUnit)unit;
                ctx.Units[unitId] = foreign.Values.ToArray();
                return DyNil.Instance;
            }

            var lay0 = unit.Layouts[0];
            
            //if yes we are in interactive mode and need to check if the size
            //of global layout (for global variables) has changes
            if (ctx.Units[0] != null && lay0.Size > ctx.Units[0].Length)
            {
                var mems = new DyObject[lay0.Size];
                Array.Copy(ctx.Units[0], mems, ctx.Units[0].Length);
                ctx.Units[0] = mems;
            }

            ctx.Units[unitId] = ctx.Units[unitId] ?? new DyObject[lay0.Size];
            return ExecuteWithData(global, ctx);
        }

        internal static DyObject ExecuteWithData(DyNativeFunction function, ExecutionContext ctx)
        {
            DyObject left;
            DyObject right;
            DyObject[] locals;
            Op op;

            var types = ctx.Types;
            var unit = ctx.Composition.Units[function.UnitId];
            var ops = unit.Ops;
            var layout = unit.Layouts[function.FunctionId];
            var offset = layout.Address;
            var evalStack = new EvalStack(layout.StackSize);

            if (function.FunctionId == 0)
                locals = ctx.Units[function.UnitId];
            else if (function.Locals != null)
            {
                locals = function.Locals;

                if (function.TypeId == StandardType.Iterator)
                    offset = function.PreviousOffset;
            }
            else
                locals = new DyObject[layout.Size];

            var captures = function.Captures;

            CYCLE:
            {
                op = ops[offset];
                offset++;

                switch (op.Code)
                {
                    case OpCode.Nop:
                        break;
                    case OpCode.This:
                        evalStack.Push(function.Self ?? DyNil.Instance);
                        break;
                    case OpCode.Term:
                        if (evalStack.Size > 1 || evalStack.Size == 0)
                            throw new DyRuntimeException(RuntimeErrors.StackCorrupted);
                        ctx.Units[function.UnitId] = locals;
                        return evalStack.Pop();
                    case OpCode.Pop:
                        evalStack.PopVoid();
                        break;
                    case OpCode.PushNil:
                        evalStack.Push(DyNil.Instance);
                        break;
                    case OpCode.PushNilT:
                        evalStack.Push(DyNil.Terminator);
                        break;
                    case OpCode.PushCh:
                        evalStack.Push(unit.IndexedChars[op.Data]);
                        break;
                    case OpCode.PushI1_1:
                        evalStack.Push(DyBool.True);
                        break;
                    case OpCode.PushI1_0:
                        evalStack.Push(DyBool.False);
                        break;
                    case OpCode.PushI8_1:
                        evalStack.Push(DyInteger.One);
                        break;
                    case OpCode.PushI8_0:
                        evalStack.Push(DyInteger.Zero);
                        break;
                    case OpCode.PushI8:
                        evalStack.Push(unit.IndexedIntegers[op.Data]);
                        break;
                    case OpCode.PushR8_0:
                        evalStack.Push(DyFloat.Zero);
                        break;
                    case OpCode.PushR8:
                        evalStack.Push(unit.IndexedFloats[op.Data]);
                        break;
                    case OpCode.PushStr:
                        evalStack.Push(unit.IndexedStrings[op.Data]);
                        break;
                    case OpCode.Poploc:
                        locals[op.Data] = evalStack.Pop();
                        break;
                    case OpCode.Pushloc:
                        evalStack.Push(locals[op.Data]);
                        break;
                    case OpCode.Pushvar:
                        right = captures[captures.Count - (op.Data & byte.MaxValue)][op.Data >> 8];
                        evalStack.Push(right);
                        break;
                    case OpCode.Pushext:
                        evalStack.Push(ctx.Units[unit.UnitIds[op.Data & byte.MaxValue]][op.Data >> 8]);
                        break;
                    case OpCode.Popvar:
                        captures[captures.Count - (op.Data & byte.MaxValue)][op.Data >> 8] = evalStack.Pop();
                        break;
                    case OpCode.Br:
                        offset = op.Data;
                        break;
                    case OpCode.Brtrue:
                        if (evalStack.Pop().GetBool())
                            offset = op.Data;
                        break;
                    case OpCode.Brfalse:
                        if (!evalStack.Pop().GetBool())
                            offset = op.Data;
                        break;
                    case OpCode.Shl:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].ShiftLeft(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Shr:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].ShiftRight(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.And:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].And(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Or:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Or(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Xor:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Xor(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Add:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Add(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Sub:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Sub(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Mul:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Mul(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Div:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Div(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Rem:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Rem(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Eq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Eq(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.NotEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Neq(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Gt:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Gt(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Lt:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Lt(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.GtEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Gte(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.LtEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Lte(left, right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Neg:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Neg(right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Not:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Not(right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.BitNot:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].BitwiseNot(right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Len:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Length(right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Dup:
                        evalStack.Dup();
                        break;
                    case OpCode.Ret:
                        function.Locals = null;
                        if (ctx.CallStack.Count > 0)
                            ctx.CallStack.Pop();
                        return evalStack.Pop();
                    case OpCode.Fail:
                        ctx.Error = Err.UserCode(evalStack.Pop().ToString());
                        ProcessError(ctx, function, ref offset);
                        break;
                    case OpCode.NewIter:
                        evalStack.Push(DyIterator.CreateIterator(function.UnitId, op.Data, captures, locals));
                        break;
                    case OpCode.NewFun:
                        right = evalStack.Peek();
                        evalStack.Replace(DyNativeFunction.Create(function.UnitId, op.Data, (int)right.GetInteger(), captures, locals));
                        break;
                    case OpCode.NewFunV:
                        right = evalStack.Peek();
                        evalStack.Replace(DyNativeFunction.Create(function.UnitId, op.Data, (int)right.GetInteger(), captures, locals, true));
                        break;
                    case OpCode.Call:
                        {
                            right = evalStack.Pop();
                            if (right.TypeId != StandardType.Function && right.TypeId != StandardType.Iterator)
                            {
                                ctx.Error = Err.NotFunction(types[right.TypeId].TypeName);
                                ProcessError(ctx, function, ref offset);
                                break;
                            }

                            if (right is DyNativeFunction callFun)
                            {
                                layout = ctx.Composition.Units[callFun.UnitId].Layouts[callFun.FunctionId];

                                if ((op.Data > callFun.ParameterNumber && !callFun.IsVariadic) || op.Data < callFun.ParameterNumber)
                                {
                                    ctx.Error = Err.WrongNumberOfArguments(callFun.GetFunctionName(ctx), callFun.ParameterNumber, op.Data);
                                    ProcessError(ctx, function, ref offset);
                                    break;
                                }

                                if (callFun.TypeId != StandardType.Iterator)
                                    callFun.Locals = new DyObject[layout.Size];

                                var arr = default(DyObject[]);

                                if (callFun.IsVariadic)
                                {
                                    arr = new DyObject[op.Data - callFun.ParameterNumber];
                                    callFun.Locals[callFun.ParameterNumber] = DyTuple.Create(arr);
                                }

                                for (var i = op.Data; i > 0; i--)
                                {
                                    if (i <= callFun.ParameterNumber)
                                        callFun.Locals[i - 1] = evalStack.Pop();
                                    else
                                        arr[i - callFun.ParameterNumber - 1] = evalStack.Pop();
                                }

                                ctx.CallStack.Push((long)offset | (long)function.UnitId << 32);
                                evalStack.Push(ExecuteWithData(callFun, ctx));
                            }
                            else
                                evalStack.Push(CallExternalFunction(op, offset, evalStack, function, (DyFunction)right, ctx));

                            if (ctx.Error != null)
                                ProcessError(ctx, function, ref offset, evalStack);
                        }
                        break;
                    case OpCode.GetMember:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].GetMemberOp(right, op.Data, unit, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.SetMember:
                        right = evalStack.Pop();
                        if (op.Data >= StandardType.All.Count)
                            types[ctx.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].TypeIds[op.Data >> 8]]
                                .SetMemberOp(ctx.AUX, right, unit, ctx);
                        else
                            types[op.Data].SetMemberOp(ctx.AUX, right, unit, ctx);
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Get:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        evalStack.Push(right.GetItem(left, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Set:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        right.SetItem(left, evalStack.Pop(), ctx);
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Str:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].ToString(right, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.RunMod:
                        ExecuteModule(unit.UnitIds[op.Data], ctx);
                        evalStack.Push(new DyModule(ctx.Composition.Units[unit.UnitIds[op.Data]], ctx.Units[unit.UnitIds[op.Data]]));
                        break;
                    case OpCode.Type:
                        evalStack.Replace(types[evalStack.Peek().TypeId]);
                        break;
                    case OpCode.Tag:
                        evalStack.Replace(new DyLabel(unit.IndexedStrings[op.Data].Value, evalStack.Peek()));
                        break;
                    case OpCode.Yield:
                        function.PreviousOffset = offset++;
                        function.Locals = locals;
                        if (ctx.CallStack.Count > 0)
                            ctx.CallStack.Pop();
                        return evalStack.Pop();
                    case OpCode.Brterm:
                        if (ReferenceEquals(evalStack.Peek(), DyNil.Terminator))
                            offset = op.Data;
                        break;
                    case OpCode.Briter:
                        if (evalStack.Peek().TypeId == StandardType.Iterator)
                            offset = op.Data;
                        break;
                    case OpCode.Aux:
                        ctx.AUX = op.Data;
                        break;
                }
            }
            goto CYCLE;
        }

        private static void ProcessError(ExecutionContext ctx, DyNativeFunction currentFunc, ref int offset, EvalStack evalStack = null)
        {
            if (evalStack != null && evalStack.Size > 0)
                evalStack.PopVoid();

            var ex = CreateException(ctx.Error, offset, currentFunc.UnitId, ctx);
            ctx.Error = null;
            throw ex;
        }

        private static DyObject CallExternalFunction(Op op, int offset, EvalStack evalStack, DyNativeFunction caller, DyFunction fun, ExecutionContext ctx)
        {
            var arr = new DyObject[op.Data];
            for (var i = op.Data - 1; i > -1; i--)
                arr[i] = evalStack.Pop();

            if (op.Data < fun.ParameterNumber)
                for (var i = op.Data; i < fun.ParameterNumber; i++)
                    arr[i] = DyNil.Instance;

#if !DEBUG
            try
#endif
            {
                return fun.Call(ctx, arr);
            }
#if !DEBUG
            catch (Exception ex)
            {
                throw CreateException(Err.ExternalFunctionFailure(fun.GetFunctionName(ctx), ex.Message),
                    offset - 1, caller.UnitId, ctx, ex);
            }
#endif
        }

        private static Stack<StackPoint> Dump(CallStack callStack)
        {
            var st = new Stack<StackPoint>();

            for (var i = 0; i < callStack.Count; i++)
            {
                var cm = callStack[i];
                st.Push(new StackPoint((int)(cm & int.MaxValue), (int)(cm >> 32)));
            }

            return st;
        }

        private static DyCodeException CreateException(DyError err, int offset, int moduleHandle, ExecutionContext ctx, Exception ex = null)
        {
            var dump = Dump(ctx.CallStack);
            dump.Push(new StackPoint(offset, moduleHandle));
            var deb = new DyDebugger(ctx.Composition);
            var cs = deb.BuildCallStack(dump);
            return new DyCodeException(err, cs, ex);
        }

        public static IEnumerable<RuntimeVar> DumpVariables(ExecutionContext ctx)
        {
            foreach (var v in ctx.Composition.Units[0].GlobalScope.EnumerateVars())
                yield return new RuntimeVar(v.Key, ctx.Units[0][v.Value.Address]);
        }
    }
}
