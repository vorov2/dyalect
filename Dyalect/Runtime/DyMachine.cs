using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System.Linq;
namespace Dyalect.Runtime;

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
        
        DyObject? first, second = null, third = null;
        DyClassInfo clsInfo;
        Op op;
        DyFunction callFun;

        PROLOGUE:
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
                case OpCode.Debug:
                    break;
                case OpCode.This:
                    evalStack.Push(function.Self!);
                    break;
                case OpCode.Unbox:
                    evalStack.Push(function.Self is DyClass c ? c.Fields : function.Self!);
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
                    evalStack.Push(True);
                    break;
                case OpCode.PushI1_0:
                    evalStack.Push(False);
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
                    second = function.Captures[^(op.Data & byte.MaxValue)][op.Data >> 8];
                    evalStack.Push(second);
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
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].ShiftLeft(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Shr:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].ShiftRight(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.And:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].And(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Or:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Or(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Xor:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Xor(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Add:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Add(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Sub:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Sub(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Mul:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Mul(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Div:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Div(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Rem:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Rem(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Eq:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Eq(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.NotEq:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Neq(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Gt:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Gt(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Lt:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Lt(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.GtEq:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Gte(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.LtEq:
                    second = evalStack.Pop();
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Lte(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Neg:
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Neg(ctx, first));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Not:
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Not(ctx, first));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.BitNot:
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].BitwiseNot(ctx, first));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Len:
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Length(ctx, first));
                    if (ctx.Error is not null) goto HANDLE;
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
                        second = evalStack.Pop();
                        if (ctx.Error is null) ctx.Error = second.ToError();
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
                    second = evalStack.Peek();
                    if (second.TypeId == Dy.TypeInfo)
                        evalStack.Replace(((DyTypeInfo)second).HasStaticMember(unit.Strings[op.Data], ctx));
                    else
                        evalStack.Replace(types[second.TypeId].HasInstanceMember(second, unit.Strings[op.Data], ctx));
                    if (ctx.Error is not null) goto CATCH;
                    break;
                case OpCode.GetMember:
                    second = evalStack.Peek();
                    if (second.TypeId == Dy.TypeInfo)
                        evalStack.Replace(((DyTypeInfo)second).GetStaticMember(unit.Strings[op.Data], ctx));
                    else
                        evalStack.Replace(types[second.TypeId].GetInstanceMember(second, unit.Strings[op.Data], ctx));
                    if (ctx.Error is not null) goto CATCH;
                    break;
                case OpCode.SetMemberS:
                    first = evalStack.Pop();
                    second = evalStack.Pop();
                    ((DyTypeInfo)first).SetStaticMember(ctx, unit.Strings[op.Data], (DyFunction)second);
                    if (ctx.Error is not null) goto CATCH;
                    break;
                case OpCode.SetMember:
                    first = evalStack.Pop();
                    second = evalStack.Pop();
                    ((DyTypeInfo)first).SetInstanceMember(ctx, unit.Strings[op.Data], (DyFunction)second);
                    if (ctx.Error is not null) goto CATCH;
                    break;
                case OpCode.Mixin:
                    first = evalStack.Pop();
                    second = evalStack.Pop();
                    ((DyTypeInfo)second).Mixin(ctx, (DyTypeInfo)first);
                    if (ctx.Error is not null) goto CATCH;
                    break;
                case OpCode.Get:
                    second = evalStack.Pop();
                    first = evalStack.Pop();
                    evalStack.Push(types[first.TypeId].Get(ctx, first, second));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Set:
                    second = evalStack.Pop();
                    first = evalStack.Pop();
                    third = evalStack.Pop();
                    evalStack.Push(types[first.TypeId].Set(ctx, first, second, third));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.Contains:
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Contains(ctx, first, unit.Objects[op.Data]));
                    if (ctx.Error is not null)
                    {
                        second = unit.Objects[op.Data];
                        goto HANDLE;
                    }
                    break;
                case OpCode.Str:
                    first = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].ToString(ctx, first));
                    if (ctx.Error is not null) goto HANDLE;
                    break;
                case OpCode.RunMod:
                    ExecuteModule(unit.UnitIds[op.Data], ctx);
                    evalStack.Push(new DyModule(ctx.RuntimeContext.Composition.Units[unit.UnitIds[op.Data]], ctx.RuntimeContext.Units[unit.UnitIds[op.Data]]));
                    break;
                case OpCode.Type:
                    evalStack.Push(types[op.Data]);
                    break;
                case OpCode.Annot:
                    first = evalStack.Pop();
                    second = evalStack.Peek();
                    ((DyLabel)second).AddTypeAnnotation((DyTypeInfo)first);
                    break;
                case OpCode.Tag:
                    evalStack.Replace(new DyLabel((string)unit.Strings[op.Data], evalStack.Peek()));
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
                    if (evalStack.Peek().TypeId == Dy.Iterator)
                        offset = op.Data;
                    break;
                case OpCode.GetIter:
                    second = evalStack.Peek();
                    if (second is DyIterator it)
                        evalStack.Replace(it.GetIteratorFunction());
                    else
                    {
                        ctx.InvalidType(Dy.Iterator, second);
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
                        second = evalStack.Peek();
                        if (second.TypeId != Dy.Function)
                        {
                            if (second.TypeId == Dy.TypeInfo && second is DyTypeInfo ti)
                            {
                                second = ti.GetStaticMember(ti.ReflectedTypeName, ctx);
                                if (ctx.Error is not null) goto CATCH;
                                evalStack.Replace(second);
                                goto case OpCode.FunPrep;
                            }

                            second = types[second.TypeId].GetInstanceMember(second, Builtins.Call, ctx);

                            if (!ctx.HasErrors && second.TypeId != Dy.Function)
                                ctx.InvalidType(Dy.Function, second);

                            if (ctx.Error is not null) goto CATCH;
                            evalStack.Replace(second);
                        }

                        callFun = (DyFunction)second;

                        if (callFun.VarArgIndex == -1)
                        {
                            if (op.Data > callFun.Parameters.Length)
                            {
                                ctx.TooManyArguments(callFun.FunctionName, callFun.Parameters.Length, op.Data);
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
                                varArgs: op.Data == 0 ? null : new DyObject[op.Data]
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
                        var fn = (DyFunction)evalStack.Peek(2);
                        var idx = fn.GetParameterIndex((string)unit.Strings[op.Data]);
                        if (idx == -1)
                        {
                            if (locs.VarArgsIndex > -1 && fn.Parameters.Length == 1)
                            {
                                locs.VarArgs![locs.VarArgsSize++] = new DyLabel((string)unit.Strings[op.Data], evalStack.Pop());
                                break;
                            }

                            ctx.ArgumentNotFound(((DyFunction)evalStack.Peek(2)).FunctionName, (string)unit.Strings[op.Data]);
                            goto CATCH;
                        }
                        if (idx == locs.VarArgsIndex)
                        {
                            Push(locs, evalStack.Pop(), ctx);
                            if (ctx.Error is not null) goto CATCH;
                        }
                        else
                        {
                            if (locs.Locals[idx] is not null)
                            {
                                ctx.MultipleValuesForArgument(((DyFunction)evalStack.Peek(2)).FunctionName, (string)unit.Strings[op.Data]);
                                goto CATCH;
                            }

                            locs.Locals[idx] = evalStack.Pop();
                        }
                    }
                    break;
                case OpCode.StdCall_0:
                    ctx.CallStack.Push(new Caller(function, offset, evalStack, locals));
                    function = (DyNativeFunction)evalStack.Pop();
                    locals = function.CreateLocals(ctx);
                    goto PROLOGUE;
                case OpCode.StdCall_1:
                    ctx.CallStack.Push(new Caller(function, offset, evalStack, locals));
                    function = (DyNativeFunction)evalStack.Pop();
                    locals = function.CreateLocals(ctx);
                    locals[0] = evalStack.Pop();
                    goto PROLOGUE;
                case OpCode.StdCall:
                    {
                        ctx.CallStack.Push(new Caller(function, offset, evalStack, locals));
                        function = (DyNativeFunction)evalStack.Pop();
                        locals = function.CreateLocals(ctx);
                        for (var i = 0; i < op.Data; i++)
                            locals[i] = evalStack.Pop();
                        goto PROLOGUE;
                    }
                case OpCode.FunCall:
                    {
                        callFun = (DyFunction)evalStack.Pop();

                        if (op.Data != callFun.Parameters.Length || callFun.VarArgIndex > -1)
                        {
                            FillDefaults(ctx.PeekArguments(), callFun, ctx);
                            if (ctx.Error is not null) goto CATCH;
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
                            second = CallExternalFunction(callFun, ctx);
                            if (ctx.Error is not null)
                                goto CATCH;
                            else
                            {
                                evalStack.Push(second);
                                ctx.CallStack.Pop();
                            }
                        }
                    }
                    break;
                case OpCode.NewArgs:
                    evalStack.Push(op.Data == 0 ? DyTuple.Empty : MakeTuple(evalStack, op.Data, true));
                    break;
                case OpCode.NewTuple:
                    evalStack.Push(op.Data == 0 ? DyTuple.Empty : MakeTuple(evalStack, op.Data, false));
                    break;
                case OpCode.TypeCheck:
                    first = evalStack.Pop();
                    second = evalStack.Pop();
                    evalStack.Push(types[second.TypeId].CheckType((DyTypeInfo)first));
                    break;
                case OpCode.CtorCheck:
                    second = evalStack.Peek();
                    evalStack.Replace(second is IProduction cc && cc.Constructor == unit.Strings[op.Data]);
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
                    second = evalStack.Pop();
                    first = evalStack.Pop();
                    evalStack.Push(new DyClass((DyClassInfo)second, (string)unit.Strings[op.Data], (DyTuple)first, unit));
                    break;
                case OpCode.NewType:
                    clsInfo = new DyClassInfo((string)unit.Strings[op.Data], types.Count);
                    types.Add(clsInfo);
                    evalStack.Push(clsInfo);
                    break;
                case OpCode.Mut:
                    ((DyLabel)evalStack.Peek()).Mutable = true;
                    break;
                case OpCode.NewCast:
                    first = evalStack.Pop();
                    second = evalStack.Pop();
                    ((DyTypeInfo)first).SetCastFunction((DyTypeInfo)second, (DyFunction)evalStack.Pop());
                    break;
                case OpCode.Cast:
                    first = evalStack.Pop();
                    second = evalStack.Peek();
                    evalStack.Replace(types[first.TypeId].Cast(ctx, first, second));
                    if (ctx.Error is not null) goto CATCH;
                    break;
            }
        }
        goto CYCLE;
    HANDLE:
        evalStack.Pop();
        if (TryCall(ctx, offset, ref second, ref third, ref function, ref locals, ref evalStack))
            goto PROLOGUE;
    CATCH:
        offset = ThrowIf(ctx, offset, ref function, ref locals, ref evalStack);
        evalStack.Clear();
        unit = ctx.RuntimeContext.Composition.Units[function.UnitId];
        ops = unit.Ops;
        evalStack.Push(ctx.Error!);
        ctx.Error = null;
        goto CYCLE;
    }

    private static void Push(ExecutionContext.ArgContainer container, DyObject value, ExecutionContext ctx)
    {
        if (container.VarArgsSize != 0)
            ctx.TooManyArguments();

        if (value.TypeId is Dy.Array)
        {
            var xs = (DyCollection)value;
            container.VarArgs = xs.ToArray();
            container.VarArgsSize = container.VarArgs.Length;
        }
        else if (value.TypeId is Dy.Tuple)
        {
            var xs = (DyTuple)value;
            container.VarArgs = xs.IsVarArg ? xs.UnsafeAccess() : xs.GetValuesWithLabels();
            container.VarArgsSize = container.VarArgs.Length;
        }
        else if (value.TypeId is Dy.Iterator or Dy.Set)
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
            return func.FastCall(ctx, ctx.PopArguments().Locals);
        }
        catch (IterationException)
        {
            return ctx.CollectionModified();
        }
        catch (FormatException)
        {
            return ctx.ParsingFailed();
        }
        catch (TimeoutException)
        {
            return ctx.Timeout();
        }
        catch (Exception ex)
        {
            (ctx.Error, ctx.Trace) = GetErrorInformation(func, ex);
            return Nil;
        }
    }

    private static DyTuple MakeTuple(EvalStack stack, int size, bool vararg)
    {
        var arr = new DyObject[size];
        var mutable = false;

        for (var i = 0; i < size; i++)
        {
            var e = stack.Pop();
            arr[arr.Length - i - 1] = e;

            if (!mutable && e is DyLabel la && la.Mutable)
                mutable = true;
        }

        return new DyTuple(arr, mutable, vararg);
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
