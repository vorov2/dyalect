using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public static partial class DyMachine
    {
        private static DyNativeFunction Global(int unitId) =>
            new(null, unitId, 0, FastList<DyObject[]>.Empty, DyType.Function, -1);

        public static ExecutionContext CreateExecutionContext(UnitComposition composition) =>
            new(new(), new(composition));

        public static ExecutionResult Execute(ExecutionContext ctx)
        {
            var res = ExecuteModule(0, ctx);
            var retval = ExecutionResult.Fetch(0, res, ctx);
            return retval;
        }

        private static DyObject? ExecuteModule(int unitId, ExecutionContext ctx)
        {
            var unit = ctx.RuntimeContext.Composition.Units[unitId];

            if (unit.Layouts.Count == 0) //This is a foreign module
            {
                if (ctx.RuntimeContext.Units[unitId] is null)
                {
                    var foreign = (ForeignUnit)unit;
                    foreign.Initialize(ctx);
                    ctx.RuntimeContext.Units[unitId] = foreign.Values.ToArray();
                }

                return DyNil.Instance;
            }

            var lay0 = unit.Layouts[0];

            //if yes we are in interactive mode and need to check if the size
            //of global layout (for global variables) has changed
            if (ctx.RuntimeContext.Units[0] is not null && lay0.Size > ctx.RuntimeContext.Units[0].Length)
            {
                var mems = new DyObject[lay0.Size];
                Array.Copy(ctx.RuntimeContext.Units[0], mems, ctx.RuntimeContext.Units[0].Length);
                ctx.RuntimeContext.Units[0] = mems;
            }

            if (unitId != 0 && ctx.RuntimeContext.Units[unitId] is not null)
                return null;

            ctx.CatchMarks.Push(null!);
            ctx.RuntimeContext.Units[unitId] = ctx.RuntimeContext.Units[unitId] ?? new DyObject[lay0.Size];
            return ExecuteWithData(Global(unitId), null!, ctx);
        }

        internal static DyObject ExecuteWithData(DyNativeFunction function, DyObject[] locals, ExecutionContext ctx)
        {
            DyObject left;
            DyObject right;
            Op op;
            DyFunction callFun;
            var types = ctx.RuntimeContext.Types;

            PROLOGUE:
            var jumper = -1;
            var unit = ctx.RuntimeContext.Composition.Units[function.UnitId];
            var ops = unit.Ops;
            var layout = unit.Layouts[function.FunctionId];
            var offset = layout.Address;
            var evalStack = new EvalStack(layout.StackSize);
            ctx.CatchMarks.Push(function.CatchMarks); //Makes sense for iterators

            if (function.FunctionId == 0)
                locals = ctx.RuntimeContext.Units[function.UnitId];
            else if (function.Locals is not null)
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
                        evalStack.Push(function.Self!.GetTaggedValue());
                        break;
                    case OpCode.Term:
                        if (evalStack.Size > 1 || evalStack.Size == 0)
                            throw new DyRuntimeException(RuntimeErrors.StackCorrupted);
                        ctx.RuntimeContext.Units[function.UnitId] = locals;
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
                    case OpCode.PushR8_1:
                        evalStack.Push(DyFloat.One);
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
                        right = function.Captures[^(op.Data & byte.MaxValue)][op.Data >> 8];
                        evalStack.Push(right);
                        break;
                    case OpCode.Pushext:
                        evalStack.Push(ctx.RuntimeContext.Units[unit.UnitIds[op.Data & byte.MaxValue]][op.Data >> 8]);
                        break;
                    case OpCode.Popvar:
                        function.Captures[^(op.Data & byte.MaxValue)][op.Data >> 8] = evalStack.Pop();
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
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Shr:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].ShiftRight(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.And:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].And(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Or:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Or(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Xor:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Xor(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Add:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Add(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Sub:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Sub(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Mul:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Mul(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Div:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Div(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Rem:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Rem(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH; ;
                        break;
                    case OpCode.Eq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Eq(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.NotEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Neq(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Gt:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Gt(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Lt:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Lt(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.GtEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Gte(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.LtEq:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Lte(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Neg:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Neg(ctx, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Not:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Not(ctx, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.BitNot:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].BitwiseNot(ctx, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Len:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].Length(ctx, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Dup:
                        evalStack.Dup();
                        break;
                    case OpCode.Ret:
                        ctx.CatchMarks.Pop();
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
                            unit = ctx.RuntimeContext.Composition.Units[function.UnitId];
                            ops = unit.Ops;
                            evalStack = cp.EvalStack;
                            goto CYCLE;
                        }
                        else
                            return evalStack.Pop();
                    case OpCode.IsNull:
                        evalStack.Push(evalStack.Pop() is null);
                        break;
                    case OpCode.Fail:
                        {
                            right = evalStack.Pop();
                            ctx.Error = right is DyError e ? e : new DyError(DyErrorCode.UnexpectedError, right);
                            ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                            goto CATCH;
                        }
                    case OpCode.FailSys:
                        ctx.Error = new((DyErrorCode)op.Data);
                        ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                        goto CATCH;
                    case OpCode.NewIter:
                        evalStack.Push(DyIterator.Create(function.UnitId, op.Data, function.Captures, locals));
                        break;
                    case OpCode.NewFun:
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, function.Captures, locals));
                        break;
                    case OpCode.NewFunV:
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, function.Captures, locals, ctx.AUX));
                        break;
                    case OpCode.HasMember:
                        right = evalStack.Peek();
                        if (right.TypeId == DyType.TypeInfo)
                            evalStack.Replace(((DyTypeInfo)right).HasStaticMember(unit.IndexedStrings[op.Data].Value, ctx));
                        else
                            evalStack.Replace(types[right.TypeId].HasInstanceMember(right, unit.IndexedStrings[op.Data].Value, ctx));
                        break;
                    case OpCode.GetMember:
                        right = evalStack.Peek();
                        if (right.TypeId == DyType.TypeInfo)
                            evalStack.Replace(((DyTypeInfo)right).GetStaticMember(unit.IndexedStrings[op.Data].Value, ctx));
                        else
                            evalStack.Replace(types[right.TypeId].GetInstanceMember(right, unit.IndexedStrings[op.Data].Value, ctx));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.SetMemberS:
                        right = evalStack.Pop();
                        types[ctx.RuntimeContext.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].Types[op.Data >> 8].Id]
                            .SetStaticMember(unit.IndexedStrings[ctx.AUX].Value, right);
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.SetMemberST:
                        right = evalStack.Pop();
                        types[op.Data].SetStaticMember(unit.IndexedStrings[ctx.AUX].Value, right);
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.SetMember:
                        right = evalStack.Pop();
                        types[ctx.RuntimeContext.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].Types[op.Data >> 8].Id]
                            .SetInstanceMember(unit.IndexedStrings[ctx.AUX].Value, right);
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.SetMemberT:
                        right = evalStack.Pop();
                        types[op.Data].SetInstanceMember(unit.IndexedStrings[ctx.AUX].Value, right);
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Get:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        evalStack.Push(types[right.TypeId].Get(ctx, right, left));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Set:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        types[right.TypeId].Set(ctx, right, left, evalStack.Pop());
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.HasField:
                        right = evalStack.Peek();
                        evalStack.Replace(right.HasItem(unit.IndexedStrings[op.Data].Value, ctx));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.Str:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].ToString(ctx, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.RunMod:
                        ExecuteModule(unit.UnitIds[op.Data], ctx);
                        evalStack.Push(new DyModule(ctx.RuntimeContext.Composition.Units[unit.UnitIds[op.Data]], ctx.RuntimeContext.Units[unit.UnitIds[op.Data]]));
                        break;
                    case OpCode.Type:
                        evalStack.Replace(types[evalStack.Peek().TypeId]);
                        break;
                    case OpCode.TypeS:
                        evalStack.Push(types[ctx.RuntimeContext.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].Types[op.Data >> 8].Id]);
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
                            function.CatchMarks = ctx.CatchMarks.Pop();

                            if (ReferenceEquals(cp, Caller.External))
                                return evalStack.Pop();

                            cp.EvalStack.Push(evalStack.Pop());
                            function = cp.Function;
                            locals = cp.Locals;
                            offset = cp.Offset;
                            unit = ctx.RuntimeContext.Composition.Units[function.UnitId];
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
                    case OpCode.GetIter:
                        evalStack.Replace(((DyIterator)evalStack.Peek()).GetIteratorFunction());
                        break;
                    case OpCode.Aux:
                        ctx.AUX = op.Data;
                        break;
                    case OpCode.FunPrep:
                        {
                            right = evalStack.Peek();
                            if (right.TypeId != DyType.Function)
                            {
                                if (right.TypeId == DyType.TypeInfo && right is DyTypeInfo ti)
                                {
                                    right = ti.GetStaticMember(ti.TypeName, ctx);

                                    if (ctx.HasErrors)
                                    {
                                        ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                                        goto CATCH;
                                    }

                                    evalStack.Replace(right);
                                    goto case OpCode.FunPrep;
                                }
                                
                                right = types[right.TypeId].GetInstanceMember(right, Builtins.Call, ctx);
                                if (!ctx.HasErrors && right.TypeId != DyType.Function) ctx.InvalidType(right);
                                if (ctx.HasErrors)
                                {
                                    ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                                    goto CATCH;
                                }

                                evalStack.Replace(right);
                            }

                            callFun = (DyFunction)right;

                            if (op.Data > callFun.Parameters.Length && callFun.VarArgIndex == -1)
                            {
                                ctx.TooManyArguments(callFun.FunctionName, callFun.Parameters.Length, op.Data);
                                ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                                goto CATCH;
                            }

                            ctx.Arguments.Push(new ArgContainer {
                                Locals = callFun.CreateLocals(ctx),
                                VarArgsIndex = callFun.VarArgIndex,
                                VarArgs = callFun.VarArgIndex > -1 ? new() : null
                            });
                        }
                        break;
                    case OpCode.FunArgIx:
                        {
                            var locs = ctx.Arguments.Peek();
                            if (locs.VarArgsIndex > -1 && op.Data >= locs.VarArgsIndex)
                            {
                                locs.VarArgs!.Add(evalStack.Pop());
                                break;
                            }
                            locs.Locals[op.Data] = evalStack.Pop();
                        }
                        break;
                    case OpCode.FunArgNm:
                        {
                            var idx = ((DyFunction)evalStack.Peek(2)).GetParameterIndex(unit.IndexedStrings[op.Data].Value);
                            if (idx == -1)
                            {
                                ctx.ArgumentNotFound(((DyFunction)evalStack.Peek(2)).FunctionName, unit.IndexedStrings[op.Data].Value);
                                ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                                goto CATCH;
                            }
                            var locs = ctx.Arguments.Peek();
                            if (idx == locs.VarArgsIndex)
                            {
                                Push(locs, evalStack.Pop(), ctx);
                                if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper))
                                    goto CATCH;
                            }
                            else
                            {
                                if (locs.Locals[idx] is not null)
                                {
                                    ctx.MultipleValuesForArgument(((DyFunction)evalStack.Peek(2)).FunctionName, unit.IndexedStrings[op.Data].Value);
                                    if (ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper))
                                        goto CATCH;
                                }

                                locs.Locals[idx] = evalStack.Pop();
                            }
                        }
                        break;
                    case OpCode.FunCall:
                        {
                            callFun = (DyFunction)evalStack.Pop();

                            if (op.Data != callFun.Parameters.Length || callFun.VarArgIndex > -1)
                            {
                                FillDefaults(ctx.Arguments.Peek(), callFun, ctx);
                                if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                            }

                            var cp = new Caller(function, offset, evalStack, locals);
                            ctx.CallStack.Push(cp);

                            if (!callFun.IsExternal)
                            {
                                function = (DyNativeFunction)callFun;
                                locals = ctx.Arguments.Pop().Locals;
                                goto PROLOGUE;
                            }
                            else
                            {
                                right = CallExternalFunction(callFun, ctx);
                                if (ctx.Error is not null && ctx.CallStack.PopLast() && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper))
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
                        evalStack.Push(op.Data == 0 ? DyTuple.Empty : MakeTuple(evalStack, op.Data));
                        break;
                    case OpCode.TypeCheckT:
                        right = evalStack.Pop();
                        evalStack.Push(right.TypeId == op.Data);
                        break;
                    case OpCode.TypeCheck:
                        right = evalStack.Pop();
                        evalStack.Push(right.TypeId == ctx.RuntimeContext.Composition.Units[unit.UnitIds[op.Data & byte.MaxValue]].Types[op.Data >> 8].Id);
                        break;
                    case OpCode.CtorCheck:
                        evalStack.Replace(evalStack.Peek().GetConstructor(ctx) == unit.IndexedStrings[op.Data].Value);
                        break;
                    case OpCode.Start:
                        {
                            var cm = ctx.CatchMarks.Peek();
                            if (cm is null)
                                ctx.CatchMarks.Replace(cm = new());
                            cm.Push(new(op.Data, ctx.CallStack.Count));
                        }
                        break;
                    case OpCode.End:
                        ctx.CatchMarks.Peek().Pop();
                        break;
                    case OpCode.NewType:
                        evalStack.Push(new DyCustomType(unit.Types[op.Data].Id, unit.IndexedStrings[ctx.AUX].Value, (DyTuple)evalStack.Pop(), unit));
                        break;
                    case OpCode.Mut:
                        ((DyLabel)evalStack.Peek()).Mutable = true;
                        break;
                }
            }
            goto CYCLE;
        CATCH:
            {
                evalStack.Clear();
                offset = jumper;
                unit = ctx.RuntimeContext.Composition.Units[function.UnitId];
                ops = unit.Ops;
                evalStack.Push(ctx.Error!);
                ctx.Error = null;
                goto CYCLE;
            }
        }

        private static void Push(ArgContainer container, DyObject value, ExecutionContext ctx)
        {
            if (value.TypeId == DyType.Array)
                container.VarArgs!.AddRange((IEnumerable<DyObject>)value);
            else if (value.TypeId == DyType.Tuple)
                container.VarArgs!.AddRange(((DyTuple)value).Values);
            else if (value.TypeId == DyType.Iterator)
                container.VarArgs!.AddRange(DyIterator.ToEnumerable(ctx, value));
            else
                container.VarArgs!.Add(value);
        }

        private static DyObject CallExternalFunction(DyFunction func, ExecutionContext ctx)
        {
            try
            {
                return func.InternalCall(ctx, ctx.Arguments.Pop().Locals);
            }
            catch (IterationException)
            {
                return ctx.CollectionModified();
            }
            catch (Exception ex)
            {
                var dy = GetCodeException(ex);
                if (dy is not null)
                    ctx.Error = dy.Error;
                else
                    ctx.ExternalFunctionFailure(func.FunctionName, ex.Message);
                return DyNil.Instance;
            }
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
            var locals = cont.Locals;

            if (callFun.VarArgIndex > -1)
                locals[callFun.VarArgIndex] = cont.VarArgs is null ? DyTuple.Empty :
                    new DyTuple(cont.VarArgs.ToArray() ?? Array.Empty<DyObject>());

            FillDefaults(cont.Locals, callFun, ctx);
        }

        internal static void FillDefaults(DyObject[] locals, DyFunction callFun, ExecutionContext ctx)
        {
            var pars = callFun.Parameters;
            
            for (var i = 0; i < pars.Length; i++)
            {
                if (locals[i] is null)
                {
                    locals[i] = pars[i].Value!;

                    if (locals[i] is null)
                    {
                        ctx.RequiredArgumentMissing(callFun.FunctionName, pars[i].Name);
                        return;
                    }
                }
            }
        }
    }
}
