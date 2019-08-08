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
        private static DyNativeFunction Global(int unitId) => 
            new DyNativeFunction(null, unitId, 0, FastList<DyObject[]>.Empty, DyType.Function, -1, AutoKind.None);

        public static ExecutionContext CreateExecutionContext(UnitComposition composition)
        {
            return new ExecutionContext(new CallStack(), composition);
        }

        public static ExecutionResult Execute(ExecutionContext ctx)
        {
            var res = ExecuteModule(0, ctx);
            var retval = ExecutionResult.Fetch(0, res, ctx);
            return retval;
        }

        private static DyObject ExecuteModule(int unitId, ExecutionContext ctx)
        {
            var unit = ctx.Composition.Units[unitId];

            if (unit.Layouts.Count == 0)
            {
                if (ctx.Units[unitId] == null)
                {
                    var foreign = (ForeignUnit)unit;
                    foreign.Execute(ctx);
                    ctx.Units[unitId] = foreign.Values.ToArray();
                }

                return DyNil.Instance;
            }

            var lay0 = unit.Layouts[0];
            
            //if yes we are in interactive mode and need to check if the size
            //of global layout (for global variables) has changed
            if (ctx.Units[0] != null && lay0.Size > ctx.Units[0].Length)
            {
                var mems = new DyObject[lay0.Size];
                Array.Copy(ctx.Units[0], mems, ctx.Units[0].Length);
                ctx.Units[0] = mems;
            }

            if (unitId != 0 && ctx.Units[unitId] != null)
                return null;

            ctx.Units[unitId] = ctx.Units[unitId] ?? new DyObject[lay0.Size];
            return ExecuteWithData(Global(unitId), null, ctx);
        }

        internal static DyObject ExecuteWithData(DyNativeFunction function, DyObject[] locals, ExecutionContext ctx)
        {
            DyObject left;
            DyObject right;
            Op op;
            DyFunction callFun;
            var types = ctx.Types;

            PROLOGUE:
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

            CYCLE:
            {
                op = ops[offset];
                offset++;

                switch (op.Code)
                {
                    case OpCode.Nop:
                        break;
                    case OpCode.This:
                        evalStack.Push(function.Self);
                        break;
                    case OpCode.Unbox:
                        evalStack.Replace(evalStack.Peek().GetSelf());
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
                        right = function.Captures[function.Captures.Count - (op.Data & byte.MaxValue)][op.Data >> 8];
                        evalStack.Push(right);
                        break;
                    case OpCode.Pushext:
                        evalStack.Push(ctx.Units[unit.UnitIds[op.Data & byte.MaxValue]][op.Data >> 8]);
                        break;
                    case OpCode.Popvar:
                        function.Captures[function.Captures.Count - (op.Data & byte.MaxValue)][op.Data >> 8] = evalStack.Pop();
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
                        evalStack.Replace(types[left.TypeId].ShiftLeft(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Shr:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].ShiftRight(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.And:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].And(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Or:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Or(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Xor:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Xor(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Add:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Add(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Sub:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Sub(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Mul:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Mul(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Div:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Div(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Rem:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Rem(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH; ;
                        break;
                    case OpCode.Eq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Eq(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.NotEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Neq(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Gt:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Gt(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Lt:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Lt(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.GtEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Gte(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.LtEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Lte(ctx, left, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Neg:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Neg(ctx, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Not:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Not(ctx, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.BitNot:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].BitwiseNot(ctx, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Len:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Length(ctx, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Dup:
                        evalStack.Dup();
                        break;
                    case OpCode.Ret:
                        function.Locals = null;
                        if (ctx.CallStack.Count > 0)
                        {
                            var cp = ctx.CallStack.Pop();

                            if (ReferenceEquals(cp, Caller.External))
                                return evalStack.Pop();

                            cp.EvalStack.Push(evalStack.Pop());
                            function = cp.Function;
                            locals = cp.Locals;
                            offset = cp.Offset;
                            unit = ctx.Composition.Units[function.UnitId];
                            ops = unit.Ops;
                            evalStack = cp.EvalStack;
                            goto CYCLE;
                        }
                        else
                            return evalStack.Pop();
                    case OpCode.Fail:
                        right = evalStack.Pop();
                        DyError err = new DyUserError(right, right.ToString(ctx));
                        if (!ctx.HasErrors)
                            ctx.Error = err;
                        ProcessError(ctx, offset, ref function, ref locals, ref evalStack);
                        goto CATCH;
                    case OpCode.FailSys:
                        ctx.Error = new DyError((DyErrorCode)op.Data);
                        ProcessError(ctx, offset, ref function, ref locals, ref evalStack);
                        goto CATCH;
                    case OpCode.NewIter:
                        evalStack.Push(DyIterator.CreateIterator(function.UnitId, op.Data, function.Captures, locals));
                        break;
                    case OpCode.NewFun:
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, function.Captures, locals));
                        break;
                    case OpCode.NewFunV:
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, function.Captures, locals, ctx.AUX));
                        break;
                    case OpCode.NewFunA:
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, function.Captures, locals, -1, AutoKind.Explicit));
                        break;
                    case OpCode.HasMember:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].HasMember(right, op.Data, unit, ctx));
                        break;
                    case OpCode.GetMember:
                        right = evalStack.Peek();
                        if (right.TypeId == DyType.TypeInfo)
                            evalStack.Replace(((DyTypeInfo)right).GetStaticMember(op.Data, unit, ctx));
                        else
                            evalStack.Replace(types[right.TypeId].GetMember(right, op.Data, unit, ctx));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.SetMemberS:
                        right = evalStack.Pop();
                        types[ctx.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].Types[op.Data >> 8].Id]
                            .SetStaticMember(ctx.AUX, right, unit, ctx);
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.SetMemberST:
                        right = evalStack.Pop();
                        types[op.Data].SetStaticMember(ctx.AUX, right, unit, ctx);
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.SetMember:
                        right = evalStack.Pop();
                        types[ctx.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].Types[op.Data >> 8].Id]
                            .SetMember(ctx.AUX, right, unit, ctx);
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.SetMemberT:
                        right = evalStack.Pop();
                        types[op.Data].SetMember(ctx.AUX, right, unit, ctx);
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Get:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        evalStack.Push(types[right.TypeId].Get(ctx, right, left));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Set:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        types[right.TypeId].Set(ctx, right, left, evalStack.Pop());
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.GetIx:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Get(ctx, right, op.Data));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.SetIx:
                        right = evalStack.Pop();
                        types[right.TypeId].Set(ctx, right, op.Data, evalStack.Pop());
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.HasField:
                        right = evalStack.Peek();
                        evalStack.Replace(right.HasItem(unit.IndexedStrings[op.Data].Value, ctx) ? DyBool.True : DyBool.False);
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.Str:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].ToString(ctx, right));
                        if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                        break;
                    case OpCode.RunMod:
                        ExecuteModule(unit.UnitIds[op.Data], ctx);
                        evalStack.Push(new DyModule(ctx.Composition.Units[unit.UnitIds[op.Data]], ctx.Units[unit.UnitIds[op.Data]]));
                        break;
                    case OpCode.Type:
                        evalStack.Replace(types[evalStack.Peek().TypeId]);
                        break;
                    case OpCode.TypeS:
                        evalStack.Push(types[ctx.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].Types[op.Data >> 8].Id]);
                        break;
                    case OpCode.TypeST:
                        evalStack.Push(types[op.Data]);
                        break;
                    case OpCode.Tag:
                        evalStack.Replace(new DyLabel(unit.IndexedStrings[op.Data].Value, evalStack.Peek()));
                        break;
                    case OpCode.Yield:
                        function.PreviousOffset = offset++;
                        function.Locals = locals;
                        if (ctx.CallStack.Count > 0)
                        {
                            var cp = ctx.CallStack.Pop();

                            if (ReferenceEquals(cp, Caller.External))
                                return evalStack.Pop();

                            cp.EvalStack.Push(evalStack.Pop());
                            function = cp.Function;
                            locals = cp.Locals;
                            offset = cp.Offset;
                            unit = ctx.Composition.Units[function.UnitId];
                            ops = unit.Ops;
                            evalStack = cp.EvalStack;
                            goto CYCLE;
                        }
                        else
                            return evalStack.Pop();
                    case OpCode.Brterm:
                        if (ReferenceEquals(evalStack.Peek(), DyNil.Terminator))
                            offset = op.Data;
                        break;
                    case OpCode.Briter:
                        if (evalStack.Peek().TypeId == DyType.Iterator)
                            offset = op.Data;
                        break;
                    case OpCode.Aux:
                        ctx.AUX = op.Data;
                        break;
                    case OpCode.FunPrep:
                        {
                            right = evalStack.Peek();
                            if (right.TypeId != DyType.Function && right.TypeId != DyType.Iterator)
                            {
                                if (right.TypeId == DyType.TypeInfo && right is DyTypeInfo ti 
                                    && ctx.Composition.MembersMap.TryGetValue(ti.TypeName, out var tid))
                                {
                                    right = ti.GetStaticMember(tid, unit, ctx);

                                    if (ctx.HasErrors)
                                    {
                                        ProcessError(ctx, offset, ref function, ref locals, ref evalStack);
                                        goto CATCH;
                                    }

                                    evalStack.Replace(right);
                                    goto  case OpCode.FunPrep;
                                }

                                ctx.NotFunction(right);
                                ProcessError(ctx, offset, ref function, ref locals, ref evalStack);
                                goto CATCH;
                            }

                            callFun = (DyFunction)right;

                            if (op.Data > callFun.Parameters.Length && callFun.VarArgIndex == -1)
                            {
                                ctx.TooManyArguments(callFun.FunctionName, callFun.Parameters.Length, op.Data);
                                ProcessError(ctx, offset, ref function, ref locals, ref evalStack);
                                goto CATCH;
                            }

                            ctx.Locals.Push(new ArgContainer {
                                Locals = callFun.CreateLocals(ctx),
                                VarArgsIndex = callFun.VarArgIndex,
                                VarArgs = callFun.VarArgIndex > -1 ? new FastList<DyObject>() : null
                            });
                        }
                        break;
                    case OpCode.FunArgIx:
                        {
                            var locs = ctx.Locals.Peek();
                            if (locs.VarArgsIndex > -1 && op.Data >= locs.VarArgsIndex)
                            {
                                locs.VarArgs.Add(evalStack.Pop());
                                break;
                            }
                            locs.Locals[op.Data] = evalStack.Pop();
                        }
                        break;
                    case OpCode.FunArgNm:
                        {
                            var idx = ((DyFunction)evalStack.Peek(2)).GetParameterIndex(unit.IndexedStrings[op.Data].Value, ctx);
                            if (idx == -1)
                            {
                                ctx.ArgumentNotFound(((DyFunction)evalStack.Peek(2)).FunctionName, unit.IndexedStrings[op.Data].Value);
                                ProcessError(ctx, offset, ref function, ref locals, ref evalStack);
                                goto CATCH;
                            }
                            var locs = ctx.Locals.Peek();
                            if (idx == locs.VarArgsIndex)
                            {
                                Push(locs, evalStack.Pop(), ctx);
                                if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                            }
                            else
                                locs.Locals[idx] = evalStack.Pop();
                        }
                        break;
                    case OpCode.FunCall:
                        {
                            callFun = (DyFunction)evalStack.Pop();

                            if (op.Data != callFun.Parameters.Length || callFun.VarArgIndex > -1)
                            {
                                FillDefaults(ctx.Locals.Peek(), callFun, ctx);
                                if (ctx.Error != null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack)) goto CATCH;
                            }

                            ctx.CallStack.Push(new Caller(function, offset, evalStack, locals));

                            if (!callFun.IsExternal)
                            {
                                function = (DyNativeFunction)callFun;
                                locals = ctx.Locals.Pop().Locals;
                                goto PROLOGUE;
                            }
                            else
                            {
                                right = CallExternalFunction(callFun, ctx);
                                if (ctx.Error != null && ctx.CallStack.PopLast() && ProcessError(ctx, offset, ref function, ref locals, ref evalStack))
                                    goto CATCH;
                                else
                                {
                                    evalStack.Push(right);
                                    ctx.CallStack.Pop();
                                }
                            }
                        }
                        break;
                    case OpCode.NewTuple:
                        evalStack.Push(MakeTuple(evalStack, op.Data));
                        break;
                    case OpCode.TypeCheckT:
                        right = evalStack.Pop();
                        evalStack.Push(right.TypeId == op.Data ? DyBool.True : DyBool.False);
                        break;
                    case OpCode.TypeCheck:
                        right = evalStack.Pop();
                        evalStack.Push(right.TypeId == ctx.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].Types[op.Data >> 8].Id ? DyBool.True : DyBool.False);
                        break;
                    case OpCode.CtorCheck:
                        evalStack.Replace((DyBool)(evalStack.Peek().GetConstructorId(ctx) == op.Data));
                        break;
                    case OpCode.Start:
                        ctx.CatchMarks.Push(new CatchMark(op.Data, ctx.CallStack.Count));
                        break;
                    case OpCode.End:
                        ctx.CatchMarks.Pop();
                        break;
                    case OpCode.NewType:
                        evalStack.Replace(new DyCustomType(unit.Types[op.Data].Id, ctx.AUX, evalStack.Peek()));
                        break;
                }
            }
            goto CYCLE;
        CATCH:
            {
                evalStack.Clear();
                offset = ctx.CatchMarks.Pop().Offset;
                if (ctx.CallStack.Count > 0)
                {
                    var cp = ctx.CallStack.Peek();
                    unit = ctx.Composition.Units[function.UnitId];
                    ops = unit.Ops;
                }
                evalStack.Push(ctx.Error.GetDyObject());
                ctx.Error = null;
                goto CYCLE;
            }
        }

        private static void Push(ArgContainer container, DyObject value, ExecutionContext ctx)
        {
            if (value.TypeId == DyType.Array)
                container.VarArgs.AddRange((IEnumerable<DyObject>)value);
            else if (value.TypeId == DyType.Tuple)
                container.VarArgs.AddRange(((DyTuple)value).Values);
            else if (value.TypeId == DyType.Iterator)
                container.VarArgs.AddRange(DyIterator.Run(ctx, value));
            else
                container.VarArgs.Add(value);
        }

        private static DyObject CallExternalFunction(DyFunction func, ExecutionContext ctx)
        {
            try
            {
                return func.Call(ctx, ctx.Locals.Pop().Locals);
            }
            catch (DyIterator.IterationException)
            {
                return ctx.CollectionModified();
            }
            catch (Exception ex)
            {
                var dy = GetCodeException(ex);
                if (dy != null)
                    ctx.Error = dy.Error;
                else
                    ctx.ExternalFunctionFailure(func.FunctionName, ex.Message);
                return DyNil.Instance;
            }
        }

        private static DyCodeException GetCodeException(Exception ex)
        {
            if (ex is DyCodeException dy)
                return dy;
            if (ex.InnerException != null)
                return GetCodeException(ex.InnerException);
            return null;
        }

        private static DyTuple MakeTuple(EvalStack stack, int size)
        {
            var arr = new DyObject[size];

            for (var i = 0; i < size; i++)
                arr[arr.Length - i - 1] = stack.Pop();

            return new DyTuple(arr);
        }

        private static void FillDefaults(ArgContainer cont, DyFunction callFun, ExecutionContext ctx)
        {
            var pars = callFun.Parameters;
            var locals = cont.Locals;

            if (callFun.VarArgIndex > -1)
                locals[callFun.VarArgIndex] = cont.VarArgs == null ? null :
                    new DyTuple(cont.VarArgs.ToArray() ?? Statics.EmptyDyObjects);

            for (var i = 0; i < pars.Length; i++)
            {
                if (locals[i] == null)
                {
                    locals[i] = pars[i].Value;

                    if (locals[i] == null)
                    {
                        ctx.RequiredArgumentMissing(callFun.FunctionName, pars[i].Name);
                        return;
                    }
                }
            }
        }

        private static bool ProcessError(ExecutionContext ctx, int offset, ref DyNativeFunction function, ref DyObject[] locals, ref EvalStack evalStack)
        {
            var err = ctx.Error;
            return ThrowIf(err, offset, function.UnitId, ref function, ref locals, ref evalStack, ctx);
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
                    st.Push(new StackPoint(external: true));
                else
                    st.Push(new StackPoint(cm.Offset, cm.Function.UnitId));
            }

            return st;
        }

        private static bool ThrowIf(DyError err, int offset, int moduleHandle, ref DyNativeFunction function, 
            ref DyObject[] locals, ref EvalStack evalStack, ExecutionContext ctx)
        {
            var dump = Dump(ctx.CallStack.Clone());
            dump.Push(new StackPoint(offset, moduleHandle));

            if (FindCatch(ctx, ref function, ref locals, ref evalStack))
            {
                ctx.Error.Dump = dump;
                return true;
            }
            else
            {
                ctx.Error = null;
                var deb = new DyDebugger(ctx.Composition);
                var cs = deb.BuildCallStack(dump);
                throw new DyCodeException(err, cs, null);
            }
        }

        public static IEnumerable<RuntimeVar> DumpVariables(ExecutionContext ctx)
        {
            foreach (var v in ctx.Composition.Units[0].GlobalScope.EnumerateVars())
                yield return new RuntimeVar(v.Key, ctx.Units[0][v.Value.Address]);
        }

        private static bool FindCatch(ExecutionContext ctx, ref DyNativeFunction function, ref DyObject[] locals, ref EvalStack evalStack)
        {
            if (ctx.CatchMarks.Count == 0)
                return false;

            var mark = ctx.CatchMarks.Peek();
            var cp = default(Caller);

            while (ctx.CallStack.Count > mark.StackOffset)
            {
                cp = ctx.CallStack.Pop();

                if (ReferenceEquals(cp, Caller.External))
                    return false;
            }

            if (cp != null)
            {
                function = cp.Function;
                locals = cp.Locals;
                evalStack = cp.EvalStack;
            }

            return true;
        }
    }
}
