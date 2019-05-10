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
        private static readonly DyNativeFunction global = new DyNativeFunction(null, 0, 0, FastList<DyObject[]>.Empty, StandardType.Function);

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
                foreign.Execute(ctx);
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
            return ExecuteWithData(global, null, ctx);
        }

        internal static DyObject ExecuteWithData(DyNativeFunction function, DyObject[] locals, ExecutionContext ctx)
        {
            DyObject left;
            DyObject right;
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
                offset = function.PreviousOffset;
            }

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
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, captures, locals));
                        break;
                    case OpCode.NewFunV:
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, captures, locals, ctx.AUX));
                        break;
                    case OpCode.Call:
                        {
                            #region Obsolete
                            //right = evalStack.Pop();
                            //if (right.TypeId != StandardType.Function && right.TypeId != StandardType.Iterator)
                            //{
                            //    ctx.Error = Err.NotFunction(types[right.TypeId].TypeName);
                            //    ProcessError(ctx, function, ref offset);
                            //    break;
                            //}

                            //if (right is DyNativeFunction callFun)
                            //{
                            //    layout = ctx.Composition.Units[callFun.UnitId].Layouts[callFun.FunctionId];

                            //    if ((op.Data > callFun.ParameterNumber && !callFun.IsVariadic) || op.Data < callFun.ParameterNumber)
                            //    {
                            //        ctx.Error = Err.TooManyArguments(callFun.GetFunctionName(ctx), callFun.ParameterNumber, op.Data);
                            //        ProcessError(ctx, function, ref offset);
                            //        break;
                            //    }

                            //    var arr = default(DyObject[]);
                            //    var newLocals = layout.Size == 0 ? Statics.EmptyDyObjects : new DyObject[layout.Size];

                            //    if (callFun.IsVariadic)
                            //    {
                            //        arr = new DyObject[op.Data - callFun.ParameterNumber];
                            //        newLocals[callFun.ParameterNumber] = DyTuple.Create(arr);
                            //    }

                            //    for (var i = op.Data; i > 0; i--)
                            //    {
                            //        if (i <= callFun.ParameterNumber)
                            //            newLocals[i - 1] = evalStack.Pop();
                            //        else
                            //            arr[i - callFun.ParameterNumber - 1] = evalStack.Pop();
                            //    }

                            //    ctx.CallStack.Push((long)offset | (long)function.UnitId << 32);
                            //    evalStack.Push(ExecuteWithData(callFun, newLocals, ctx));
                            //}
                            //else
                            //    evalStack.Push(CallExternalFunction(op, offset, evalStack, function, (DyFunction)right, ctx));

                            //if (ctx.Error != null)
                            //    ProcessError(ctx, function, ref offset, evalStack);
                            #endregion
                        }
                        break;
                    case OpCode.HasMember:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].HasMember(right, op.Data, unit, ctx));
                        break;
                    case OpCode.GetMember:
                        right = evalStack.Peek();
                        if (right.TypeId == StandardType.TypeInfo)
                            evalStack.Replace(((DyTypeInfo)right).GetStaticMember(op.Data, unit, ctx));
                        else
                            evalStack.Replace(types[right.TypeId].GetMember(right, op.Data, unit, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.SetMemberS:
                        right = evalStack.Pop();
                        if (op.Data >= StandardType.TypeNames.Length)
                            types[ctx.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].TypeIds[op.Data >> 8]]
                                .SetStaticMember(ctx.AUX, right, unit, ctx);
                        else
                            types[op.Data].SetStaticMember(ctx.AUX, right, unit, ctx);
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.SetMember:
                        right = evalStack.Pop();
                        if (op.Data >= StandardType.TypeNames.Length)
                            types[ctx.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].TypeIds[op.Data >> 8]]
                                .SetMember(ctx.AUX, right, unit, ctx);
                        else
                            types[op.Data].SetMember(ctx.AUX, right, unit, ctx);
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
                    case OpCode.FunPrep:
                        {
                            right = evalStack.Peek();
                            if (right.TypeId != StandardType.Function && right.TypeId != StandardType.Iterator)
                            {
                                ctx.Error = Err.NotFunction(types[right.TypeId].TypeName);
                                ProcessError(ctx, function, ref offset);
                                break;
                            }

                            if (right is DyNativeFunction callFun)
                            {
                                layout = ctx.Composition.Units[callFun.UnitId].Layouts[callFun.FunctionId];

                                if (op.Data > callFun.Parameters.Length && callFun.VarArgIndex == -1)
                                {
                                    ctx.Error = Err.TooManyArguments(callFun.FunctionName, callFun.Parameters.Length, op.Data);
                                    ProcessError(ctx, function, ref offset);
                                    break;
                                }

                                ctx.Locals.Push(new ArgContainer {
                                    Locals = new DyObject[layout.Size],
                                    VarArgsIndex = callFun.VarArgIndex,
                                    VarArgs = callFun.VarArgIndex == -1 ? null : new FastList<DyObject>()
                                });
                            }
                            else
                                ctx.Locals.Push(new ArgContainer { Locals = new DyObject[op.Data], VarArgsIndex = int.MaxValue });
                        }
                        break;
                    case OpCode.FunArgIx:
                        if (op.Data >= ctx.Locals.Peek().VarArgsIndex)
                        {
                            ctx.Locals.Peek().VarArgs.Add(evalStack.Pop());
                            break;
                        }

                        ctx.Locals.Peek().Locals[op.Data] = evalStack.Pop();
                        break;
                    case OpCode.FunArgNm:
                        {
                            var idx = ((DyFunction)evalStack.Peek(2)).GetParameterIndex(unit.IndexedStrings[op.Data].Value, ctx);
                            if (idx == -1)
                            {
                                ctx.Error = Err.ArgumentNotFound(((DyFunction)evalStack.Peek(2)).FunctionName, unit.IndexedStrings[op.Data].Value);
                                ProcessError(ctx, function, ref offset, evalStack);
                                break;
                            }

                            ctx.Locals.Peek().Locals[idx] = evalStack.Pop();
                        }
                        break;
                    case OpCode.FunCall:
                        {
                            right = evalStack.Pop();

                            if (right is DyNativeFunction callFun)
                            {
                                if (op.Data != callFun.Parameters.Length || callFun.VarArgIndex > -1)
                                {
                                    FillDefaults(ctx.Locals.Peek(), callFun, ctx);
                                    if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                                }

                                ctx.CallStack.Push((long)offset | (long)function.UnitId << 32);
                                evalStack.Push(ExecuteWithData(callFun, ctx.Locals.Pop().Locals, ctx));
                            }
                            else
                                evalStack.Push(CallExternalFunction2(op, offset, function, (DyFunction)right, ctx));

                            if (ctx.Error != null)
                                ProcessError(ctx, function, ref offset, evalStack);
                        }
                        break;
                }
            }
            goto CYCLE;
        }

        private static void FillDefaults(ArgContainer cont, DyNativeFunction callFun, ExecutionContext ctx)
        {
            var pars = callFun.Parameters;
            var locals = cont.Locals;

            if (callFun.VarArgIndex > -1)
                locals[callFun.VarArgIndex] = DyTuple.Create(cont.VarArgs?.ToArray() ?? Statics.EmptyDyObjects);

            for (var i = 0; i < pars.Length; i++)
            {
                if (locals[i] == null)
                {
                    locals[i] = pars[i].Value;

                    if (locals[i] == null)
                    {
                        ctx.Error = Err.RequiredArgumentMissing(callFun.FunctionName, pars[i].Name);
                        return;
                    }
                }
            }
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

            if (op.Data < fun.Parameters.Length)
                for (var i = op.Data; i < fun.Parameters.Length; i++)
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

        private static DyObject CallExternalFunction2(Op op, int offset, DyNativeFunction caller, DyFunction fun, ExecutionContext ctx)
        {
#if !DEBUG
            try
#endif
            {
                return fun.Call(ctx, ctx.Locals.Pop().Locals);
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
