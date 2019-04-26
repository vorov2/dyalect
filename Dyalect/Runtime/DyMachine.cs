using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Linker;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public sealed class DyMachine
    {
        private readonly DyObject[][] units;
        private readonly FastList<DyTypeInfo> types;

        public UnitComposition Composition { get; }

        public ExecutionContext ExecutionContext { get; }

        public DyMachine(UnitComposition asm)
        {
            Composition = asm;
            var callStack = new CallStack();
            ExecutionContext = new ExecutionContext(callStack, asm);
            units = new DyObject[Composition.Units.Count][];
            types = asm.Types;
        }

        public ExecutionResult Execute()
        {
            ExecutionResult retval = null;
            ExecutionContext.Error = null;

#if !DEBUG
            Exception eex = null;

            var th = new System.Threading.Thread(() =>
            {
                try
                {
                    var res = ExecuteModule(0);
                    retval = ExecutionResult.Fetch(0, res, ExecutionContext);
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
            var res = ExecuteModule(0);
            retval = ExecutionResult.Fetch(0, res, ExecutionContext);
#endif
            return retval;
        }

        private DyObject ExecuteModule(int unitId)
        {
            var unit = Composition.Units[unitId];

            if (unit.Layouts.Count == 0)
            {
                var foreign = (ForeignUnit)unit;
                units[unitId] = foreign.Values.ToArray();
                return DyNil.Instance;
            }

            const int funcId = 0;
            var lay0 = unit.Layouts[funcId];
            
            //if yes we are in interactive mode and need to check if the size
            //of global layout (for global variables) has changes
            if (units[0] != null && lay0.Size > units[0].Length)
            {
                var mems = new DyObject[lay0.Size];
                Array.Copy(units[0], mems, units[0].Length);
                units[0] = mems;
            }

            units[unitId] = units[unitId] ?? new DyObject[lay0.Size];
            var evalStack = new EvalStack(lay0.StackSize);
            return ExecuteWithData(new DyNativeFunction(unitId, funcId, 0, this, FastList<DyObject[]>.Empty, StandardType.Function), evalStack);
        }

        internal DyObject ExecuteWithData(DyNativeFunction function, EvalStack evalStack)
        {
            DyObject left;
            DyObject right;
            DyObject[] locals;
            Op op;
            var ctx = ExecutionContext;

            var unit = Composition.Units[function.UnitId];
            var ops = unit.Ops;
            var layout = unit.Layouts[function.FunctionId];
            var offset = layout.Address;

            if (function.FunctionId == 0)
                locals = units[function.UnitId];
            else if (function.Locals != null)
            {
                locals = function.Locals;
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
                        units[function.UnitId] = locals;
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
                        evalStack.Push(units[unit.UnitIds[op.Data & byte.MaxValue]][op.Data >> 8]);
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
                        evalStack.Push(DyIterator.CreateIterator(function.UnitId, op.Data, this, captures, locals));
                        break;
                    case OpCode.NewFun:
                        right = evalStack.Peek();
                        evalStack.Replace(DyNativeFunction.Create(function.UnitId, op.Data, (int)right.GetInteger(), this, captures, locals));
                        break;
                    case OpCode.NewFunV:
                        right = evalStack.Peek();
                        evalStack.Replace(DyNativeFunction.Create(function.UnitId, op.Data, (int)right.GetInteger(), this, captures, locals, true));
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
                                layout = ctx.Assembly.Units[callFun.UnitId].Layouts[callFun.FunctionId];
                                var newStack = new EvalStack(layout.StackSize);

                                if ((op.Data > callFun.ParameterNumber && !callFun.IsVariadic) || op.Data < callFun.ParameterNumber)
                                {
                                    ctx.Error = Err.WrongNumberOfArguments(callFun.FunctionName, callFun.ParameterNumber, op.Data);
                                    ProcessError(ctx, function, ref offset);
                                    break;
                                }

                                var arr = default(DyObject[]);

                                if (callFun.IsVariadic)
                                {
                                    arr = new DyObject[op.Data - callFun.ParameterNumber];
                                    newStack.Push(DyTuple.Create(arr));
                                }

                                //Надо выровнять либо стек, либо переданные аргументы
                                for (var i = op.Data; i > 0; i--)
                                {
                                    if (i <= callFun.ParameterNumber)
                                        newStack.Push(evalStack.Pop());
                                    else
                                        arr[i - callFun.ParameterNumber - 1] = evalStack.Pop();
                                }

                                ctx.CallStack.Push(new CallPoint(offset, function.UnitId));
                                evalStack.Push(ExecuteWithData(callFun, newStack));
                            }
                            else
                                evalStack.Push(CallExternalFunction(op, offset, evalStack, function, (DyFunction)right, ctx));

                            if (ctx.Error != null)
                                ProcessError(ctx, function, ref offset, evalStack);
                        }
                        break;
                    case OpCode.TraitG:
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].GetTraitOp(right, unit.IndexedStrings[op.Data].Value, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.TraitS:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        if (op.Data >= StandardType.All.Count)
                            types[ctx.Assembly.Units[unit.UnitIds[op.Data & byte.MaxValue]].TypeIds[op.Data >> 8]]
                                .SetTraitOp(left.GetString(), right, ctx);
                        else
                            types[op.Data].SetTraitOp(left.GetString(), right, ctx);
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Get:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        evalStack.Push(right.GetItem(left, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Get0:
                        right = evalStack.Pop();
                        evalStack.Push(right.GetItem(DyInteger.Zero, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Get1:
                        right = evalStack.Pop();
                        evalStack.Push(right.GetItem(DyInteger.One, ctx));
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
                        ExecuteModule(unit.UnitIds[op.Data]);
                        evalStack.Push(new DyModule(Composition.Units[unit.UnitIds[op.Data]], units[unit.UnitIds[op.Data]]));
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
                }
            }
            goto CYCLE;
        }

        private void ProcessError(ExecutionContext ctx, DyNativeFunction currentFunc, ref int offset, EvalStack evalStack = null)
        {
            if (evalStack != null && evalStack.Size > 0)
                evalStack.PopVoid();

            var ex = CreateException(ctx.Error, offset, currentFunc.UnitId, ctx);
            ctx.Error = null;
            throw ex;
        }

        private DyObject CallExternalFunction(Op op, int offset, EvalStack evalStack, DyNativeFunction caller, DyFunction fun, ExecutionContext ctx)
        {
            var arr = new DyObject[op.Data];
            for (var i = op.Data - 1; i > -1; i--)
                arr[i] = evalStack.Pop();

            if (op.Data < fun.ParameterNumber)
                for (var i = op.Data; i < fun.ParameterNumber; i++)
                    arr[i] = DyNil.Instance;

            try
            {
                return fun.Call(ctx, arr);
            }
            catch (Exception ex)
            {
                throw CreateException(Err.ExternalFunctionFailure(fun.FunctionName, ex.Message),
                    offset - 1, caller.UnitId, ctx, ex);
            }
        }

        private static Stack<StackPoint> Dump(CallStack callStack)
        {
            var st = new Stack<StackPoint>();

            for (var i = 0; i < callStack.Count; i++)
            {
                var cm = callStack[i];
                st.Push(new StackPoint(cm.ReturnAddress, cm.UnitId));
            }

            return st;
        }

        private DyCodeException CreateException(DyError err, int offset, int moduleHandle, ExecutionContext ctx, Exception ex = null)
        {
            var dump = Dump(ctx.CallStack);
            dump.Push(new StackPoint(offset, moduleHandle));
            var deb = new DyDebugger(Composition);
            var cs = deb.BuildCallStack(dump);
            return new DyCodeException(err, cs, ex);
        }

        public IEnumerable<RuntimeVar> DumpVariables()
        {
            foreach (var v in Composition.Units[0].GlobalScope.EnumerateVars())
                yield return new RuntimeVar(v.Key, units[0][v.Value.Address]);
        }
    }
}
