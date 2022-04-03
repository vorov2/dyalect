using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Runtime
{
    public static partial class DyMachine
    {
        private const int MAX_NESTED_CALLS = 200;

        private static DyNativeFunction Global(int unitId) => new(null, unitId, 0, FastList<DyObject[]>.Empty, -1);

        public static ExecutionContext CreateExecutionContext(UnitComposition composition) =>
            new(new(), new(composition));

        public static ExecutionResult Execute(ExecutionContext ctx)
        {
            var res = ExecuteModule(0, ctx);
            var retval = ExecutionResult.Fetch(0, res, ctx);
            return retval;
        }

        private static DyObject ExecuteModule(int unitId, ExecutionContext ctx)
        {
            var unit = ctx.RuntimeContext.Composition.Units[unitId];

            if (unit.Layouts.Count == 0) //This is a foreign module
            {
                if (ctx.RuntimeContext.Units[unitId] is null) //This module is not processed yet
                {
                    var foreign = (ForeignUnit)unit;
                    foreign.Initialize(ctx);
                    ctx.RuntimeContext.Units[unitId] = foreign.Values.ToArray();
                }

                return DyNil.Instance;
            }

            var lay0 = unit.Layouts[0];

            //if yes, we are in interactive mode and need to check if the size
            //of global layout (for global variables) has changed
            if (ctx.RuntimeContext.Units[0] is not null && lay0.Size > ctx.RuntimeContext.Units[0].Length)
            {
                var mems = new DyObject[lay0.Size];
                Array.Copy(ctx.RuntimeContext.Units[0], mems, ctx.RuntimeContext.Units[0].Length);
                ctx.RuntimeContext.Units[0] = mems;
            }

            //Module is already processed, no need for further actions.
            //However if unitId is 0 and is already processed - it means that we are inside interactive
            //and should execute it one more time.
            if (unitId is not 0 && ctx.RuntimeContext.Units[unitId] is not null)
                return DyNil.Instance;

            ctx.CatchMarks.Push(null!);
            ctx.RuntimeContext.Units[unitId] = ctx.RuntimeContext.Units[unitId] ?? new DyObject[lay0.Size];
            return ExecuteWithData(Global(unitId), Array.Empty<DyObject>(), ctx);
        }

        internal static DyObject ExecuteWithData(DyNativeFunction function, DyObject[] locals, ExecutionContext ctx)
        {
            ctx.CallCnt++;

            if (ctx.CallCnt > MAX_NESTED_CALLS)
                throw new DyRuntimeException(RuntimeErrors.StackOverflow_0);
            
            DyObject left, right;
            DyClassInfo cls;
            Op op;
            DyFunction callFun;

            PROLOGUE:
            var jumper = -1;
            var unit = ctx.RuntimeContext.Composition.Units[function.UnitId];
            ctx.CallerUnitId = ctx.UnitId;
            ctx.UnitId = function.UnitId;
            var ops = unit.Ops;
            var layout = unit.Layouts[function.FunctionId];
            var offset = layout.Address;
            var evalStack = new EvalStack(layout.StackSize);
            var types = ctx.RuntimeContext.Types;
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
                        evalStack.Push(function.Self!);
                        break;
                    case OpCode.Unbox:
                        evalStack.Push(function.Self!.GetInitValue());
                        break;
                    case OpCode.Term:
                        if (evalStack.Size is > 1 or 0)
                            throw new DyRuntimeException(RuntimeErrors.StackCorrupted_0);
                        ctx.RuntimeContext.Units[function.UnitId] = locals!;
                        ctx.CallCnt--;
                        ctx.UnitId = ctx.CallerUnitId;
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
                        evalStack.Push(unit.Objects[op.Data]);
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
                        evalStack.Push(unit.Objects[op.Data]);
                        break;
                    case OpCode.PushR8_0:
                        evalStack.Push(DyFloat.Zero);
                        break;
                    case OpCode.PushR8_1:
                        evalStack.Push(DyFloat.One);
                        break;
                    case OpCode.PushR8:
                        evalStack.Push(unit.Objects[op.Data]);
                        break;
                    case OpCode.PushStr:
                        evalStack.Push(unit.Objects[op.Data]);
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
                        if (evalStack.Pop().IsTrue())
                            offset = op.Data;
                        break;
                    case OpCode.Brfalse:
                        if (evalStack.Pop().IsFalse())
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
                            {
                                ctx.CallCnt--;
                                return evalStack.Pop();
                            }

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
                        {
                            ctx.CallCnt--;
                            return evalStack.Pop();
                        }
                    case OpCode.IsNull:
                        evalStack.Push(evalStack.Pop() is null);
                        break;
                    case OpCode.Fail:
                        {
                            right = evalStack.Pop();
                            if (ctx.Error is null) ctx.Error = (DyVariant)right;
                            ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                            goto CATCH;
                        }
                    case OpCode.NewIter:
                        evalStack.Push(DyIterator.Create(function.UnitId, op.Data, function.Captures, locals));
                        break;
                    case OpCode.NewFun:
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, function.Captures, locals));
                        break;
                    case OpCode.NewFunV:
                        evalStack.Push(DyNativeFunction.Create(unit.Symbols.Functions[op.Data], unit.Id, op.Data, function.Captures, locals, ctx.RgDI));
                        break;
                    case OpCode.FunAttr:
                        ((DyFunction)evalStack.Peek()).Attr |= op.Data;
                        break;
                    case OpCode.HasMember:
                        right = evalStack.Peek();
                        if (right.TypeId == DyType.TypeInfo)
                            evalStack.Replace(((DyTypeInfo)right).HasStaticMember(unit.Strings[op.Data], ctx));
                        else
                            evalStack.Replace(types[right.TypeId].HasInstanceMember(right, unit.Strings[op.Data], ctx));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.GetMember:
                        right = evalStack.Peek();
                        if (right.TypeId == DyType.TypeInfo)
                            evalStack.Replace(((DyTypeInfo)right).GetStaticMember(unit.Strings[op.Data], ctx));
                        else
                            evalStack.Replace(types[right.TypeId].GetInstanceMember(right, unit.Strings[op.Data], ctx));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.SetMemberS:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        ((DyTypeInfo)left).SetStaticMember(ctx, unit.Strings[op.Data], (DyFunction)right);
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        break;
                    case OpCode.SetMember:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        ((DyTypeInfo)left).SetInstanceMember(ctx, unit.Strings[op.Data], (DyFunction)right);
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
                        evalStack.Replace(right.HasItem(unit.Strings[op.Data], ctx));
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
                        evalStack.Push(types[op.Data]);
                        break;
                    case OpCode.Annot:
                        left = evalStack.Pop();
                        right = evalStack.Peek();
                        ((DyLabel)right).AddTypeAnnotation((DyTypeInfo)left);
                        break;
                    case OpCode.Tag:
                        evalStack.Replace(new DyLabel(unit.Strings[op.Data], evalStack.Peek()));
                        break;
                    case OpCode.Yield:
                        function.PreviousOffset = offset++;
                        function.Locals = locals;
                        if (ctx.CallStack.Count > 0)
                        {
                            var cp = ctx.CallStack.Pop();
                            function.CatchMarks = ctx.CatchMarks.Pop();

                            if (ReferenceEquals(cp, Caller.External))
                            {
                                ctx.CallCnt--;
                                return evalStack.Pop();
                            }

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
                        {
                            ctx.CallCnt--;
                            return evalStack.Pop();
                        }
                    case OpCode.Brterm:
                        if (ReferenceEquals(evalStack.Peek(), DyNil.Terminator))
                            offset = op.Data;
                        break;
                    case OpCode.Briter:
                        if (evalStack.Peek().TypeId == DyType.Iterator)
                            offset = op.Data;
                        break;
                    case OpCode.GetIter:
                        right = evalStack.Peek();
                        if (right is DyIterator it)
                            evalStack.Replace(it.GetIteratorFunction());
                        else
                        {
                            ctx.InvalidType(DyType.Iterator, right);
                            ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                            goto CATCH;
                        }
                        break;
                    case OpCode.RgDI:
                        ctx.RgDI = op.Data;
                        break;
                    case OpCode.RgFI:
                        ctx.RgFI = op.Data;
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

                                if (!ctx.HasErrors && right.TypeId != DyType.Function)
                                    ctx.InvalidType(right);

                                if (ctx.HasErrors)
                                {
                                    ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                                    goto CATCH;
                                }

                                evalStack.Replace(right);
                            }

                            callFun = (DyFunction)right;

                            if (callFun.VarArgIndex == -1)
                            {
                                if (op.Data > callFun.Parameters.Length)
                                {
                                    ctx.TooManyArguments(callFun.FunctionName, callFun.Parameters.Length, op.Data);
                                    ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                                    goto CATCH;
                                }

                                ctx.PushArguments(
                                    locals: callFun.CreateLocals(ctx),
                                    varArgsIndex: -1
                                );
                            }
                            else
                            {
                                ctx.PushArguments(
                                    locals: callFun.CreateLocals(ctx),
                                    varArgsIndex: callFun.VarArgIndex,
                                    varArgs: op.Data == 0 ? Array.Empty<DyObject>() : new DyObject[op.Data]
                                );
                            }
                        }
                        break;
                    case OpCode.FunArgIx:
                        {
                            var locs = ctx.PeekArguments();
                            if (locs.VarArgsIndex > -1 && op.Data >= locs.VarArgsIndex)
                            {
                                locs.VarArgs![locs.VarArgsSize++] = evalStack.Pop();
                                break;
                            }
                            locs.Locals[op.Data] = evalStack.Pop();
                        }
                        break;
                    case OpCode.FunArgNm:
                        {
                            var locs = ctx.PeekArguments();
                            var idx = ((DyFunction)evalStack.Peek(2)).GetParameterIndex(unit.Strings[op.Data]);
                            if (idx == -1)
                            {
                                if (locs.VarArgsIndex > -1 && locs.Locals.Length == 1)
                                {
                                    locs.VarArgs![locs.VarArgsSize++] = new DyLabel(unit.Strings[op.Data], evalStack.Pop());
                                    break;
                                }

                                ctx.ArgumentNotFound(((DyFunction)evalStack.Peek(2)).FunctionName, unit.Strings[op.Data]);
                                ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper);
                                goto CATCH;
                            }
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
                                    ctx.MultipleValuesForArgument(((DyFunction)evalStack.Peek(2)).FunctionName, unit.Strings[op.Data]);
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
                                FillDefaults(ctx.PeekArguments(), callFun, ctx);
                                if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                            }

                            var cp = new Caller(function, offset, evalStack, locals);
                            ctx.CallStack.Push(cp);

                            if (!callFun.IsExternal)
                            {
                                function = (DyNativeFunction)callFun;
                                locals = ctx.PopArguments().Locals;
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
                        evalStack.Push(op.Data == 0 ? DyTuple.Empty : new DyTuple(MakeArray(evalStack, op.Data)));
                        break;
                    case OpCode.TypeCheck:
                        {
                            left = evalStack.Pop();
                            right = evalStack.Pop();
                            if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                            evalStack.Push(((DyTypeInfo)left).ReflectedTypeId == right.TypeId);
                        }
                        break;
                    case OpCode.CtorCheck:
                        right = evalStack.Peek();
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
                        evalStack.Replace(right.GetConstructor() == unit.Strings[op.Data]);
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
                    case OpCode.NewObj:
                        right = evalStack.Pop();
                        left = evalStack.Pop();
                        evalStack.Push(new DyClass((DyClassInfo)right, unit.Strings[op.Data], (DyTuple)left, unit));
                        break;
                    case OpCode.NewType:
                        cls = new DyClassInfo(unit.Strings[op.Data], types.Count);
                        types.Add(cls);
                        evalStack.Push(cls);
                        break;
                    case OpCode.Mut:
                        ((DyLabel)evalStack.Peek()).Mutable = true;
                        break;
                    case OpCode.NewCast:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        ((DyTypeInfo)left).SetCastFunction((DyTypeInfo)right, (DyFunction)evalStack.Pop());
                        break;
                    case OpCode.Cast:
                        left = evalStack.Pop();
                        right = evalStack.Peek();
                        evalStack.Replace(types[left.TypeId].Cast(ctx, left, right));
                        if (ctx.Error is not null && ProcessError(ctx, offset, ref function, ref locals, ref evalStack, ref jumper)) goto CATCH;
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

        private static void Push(ExecutionContext.ArgContainer container, DyObject value, ExecutionContext ctx)
        {
            if (value.TypeId is DyType.Array)
            {
                var xs = (DyCollection)value;
                container.VarArgs = xs.GetValues();
                container.VarArgsSize = container.VarArgs.Length;
            }
            else if (value.TypeId is DyType.Tuple)
            {
                var xs = (DyTuple)value;
                container.VarArgs = xs.UnsafeAccessValues();
                container.VarArgsSize = container.VarArgs.Length;
            }
            else if (value.TypeId is DyType.Iterator)
            {
                var xs = DyIterator.ToEnumerable(ctx, value).ToArray();

                if (ctx.HasErrors)
                    return;

                container.VarArgs = xs;
                container.VarArgsSize = xs.Length;
            }
            else
            {
                container.VarArgs![container.VarArgsSize++] = value;
            }
        }

        private static DyObject CallExternalFunction(DyFunction func, ExecutionContext ctx)
        {
            try
            {
                return func.InternalCall(ctx, ctx.PopArguments().Locals);
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

        private static DyObject[] MakeArray(EvalStack stack, int size)
        {
            var arr = new DyObject[size];

            for (var i = 0; i < size; i++)
                arr[arr.Length - i - 1] = stack.Pop();

            return arr;
        }

        private static void FillDefaults(ExecutionContext.ArgContainer cont, DyFunction callFun, ExecutionContext ctx)
        {
            var locals = cont.Locals;

            if (callFun.VarArgIndex > -1)
                locals[callFun.VarArgIndex] = cont.VarArgs is null ? DyTuple.Empty 
                    : new DyTuple(cont.VarArgs, cont.VarArgsSize);

            FillDefaults(cont.Locals, callFun, ctx);
        }

        internal static void FillDefaults(DyObject[] locals, DyFunction callFun, ExecutionContext ctx)
        {
            var pars = callFun.Parameters;
            
            for (var i = 0; i < pars.Length; i++)
            {
                if (locals[i] is null)
                {
                    var v = pars[i].Value;

                    if (v is null)
                    {
                        ctx.RequiredArgumentMissing(callFun.FunctionName, pars[i].Name);
                        return;
                    }

                    locals[i] = v;
                }
            }
        }
    }
}
