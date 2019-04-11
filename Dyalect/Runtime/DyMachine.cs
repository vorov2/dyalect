using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public sealed class DyMachine
    {
        private readonly DyObject[][] modules;
        private readonly FastList<DyTypeInfo> types;

        internal UnitComposition Assembly { get; }
        internal ExecutionContext ExecutionContext { get; }

        public DyMachine(UnitComposition asm)
        {
            Assembly = asm;
            var callStack = new CallStack();
            ExecutionContext = new ExecutionContext(callStack, asm);
            modules = new DyObject[Assembly.Units.Count][];
            types = asm.Types;
        }

        public ExecutionResult Execute()
        {
            var res = ExecuteModule(0);
            return ExecutionResult.Fetch(0, res);
        }

        private DyObject ExecuteModule(int moduleHandle)
        {
            var unit = Assembly.Units[moduleHandle];

            if (unit.Layouts.Count == 0)
            {
                var foreign = (ForeignUnit)unit;
                modules[moduleHandle] = foreign.Values.ToArray();
                return DyNil.Instance;
            }

            var lay0 = unit.Layouts[0];
            
            //Если да, то мы в интерактивном режиме и надо проверить, не менялся
            //ли размер, выделенный под глобальные переменные основного модуля
            if (modules[0] != null && lay0.Size > modules[0].Length)
            {
                var mems = new DyObject[lay0.Size];
                Array.Copy(modules[0], mems, modules[0].Length);
                modules[0] = mems;
            }

            var evalStack = new EvalStack(lay0.StackSize);
            return ExecuteWithData(new DyFunction(moduleHandle, 0, 0, this, FastList<DyObject[]>.Empty), evalStack);
        }

        internal DyObject ExecuteWithData(DyFunction function, EvalStack evalStack)
        {
            DyObject left;
            DyObject right;
            CallPoint cp;
            Op op;
            int opd;
            DyFunction callFun;
            var ctx = ExecutionContext;

            FPROLOG:
            var unit = Assembly.Units[function.UnitHandle];
            var ops = unit.Ops;
            var layout = unit.Layouts[function.Handle];
            var offset = layout.Address;
            var locals = function.Handle == 0 ? modules[function.UnitHandle] : new DyObject[layout.Size];
            locals = locals ?? new DyObject[layout.Size];
            var captures = function.Captures;

            CYCLE:
            {
                op = ops[offset];
                opd = op.Data;
                offset++;

                switch (op.Code)
                {
                    case OpCode.Nop:
                        break;
                    case OpCode.This:
                        evalStack.Push(function);
                        break;
                    case OpCode.Self:
                        evalStack.Push(function.Self ?? DyNil.Instance);
                        break;
                    case OpCode.Term:
                        if (evalStack.Size > 1) throw new DyRuntimeException(RuntimeErrors.StackCorrupted);
                        modules[function.UnitHandle] = locals;
                        return evalStack.Pop();
                    case OpCode.Pop:
                        evalStack.PopVoid();
                        break;
                    case OpCode.PushNil:
                        evalStack.Push(DyNil.Instance);
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
                        evalStack.Push(unit.IndexedIntegers[opd]);
                        break;
                    case OpCode.PushR8_0:
                        evalStack.Push(DyFloat.Zero);
                        break;
                    case OpCode.PushR8:
                        evalStack.Push(unit.IndexedFloats[opd]);
                        break;
                    case OpCode.PushStr:
                        evalStack.Push(unit.IndexedStrings[opd]);
                        break;
                    case OpCode.Poploc:
                        locals[opd] = evalStack.Pop();
                        break;
                    case OpCode.Pushloc:
                        evalStack.Push(locals[opd]);
                        break;
                    case OpCode.Pushvar:
                        right = captures[captures.Count - (opd & byte.MaxValue)][opd >> 8];
                        evalStack.Push(right);
                        break;
                    case OpCode.Pushext:
                        evalStack.Push(modules[unit.ModuleHandles[opd & byte.MaxValue]][opd >> 8]);
                        break;
                    case OpCode.Popvar:
                        captures[captures.Count - (opd & byte.MaxValue)][opd >> 8] = evalStack.Pop();
                        break;
                    case OpCode.Br:
                        offset = opd;
                        break;
                    case OpCode.Brtrue:
                        if (evalStack.Pop().AsBool())
                            offset = opd;
                        break;
                    case OpCode.Brfalse:
                        if (!evalStack.Pop().AsBool())
                            offset = opd;
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
                        right = evalStack.Pop();
                        if (ctx.CallStack.Count == 0)
                            return right;
                        else
                            goto FEPILOG;
                    case OpCode.Fail:
                        ctx.Error = Err.UserCode(evalStack.Pop().ToString());
                        ProcessError(ctx, function, ref offset);
                        break; ;
                    case OpCode.NewFun:
                        right = evalStack.Peek();
                        var lst = new FastList<DyObject[]>(captures);
                        lst.Add(locals);
                        evalStack.Replace(new DyFunction(function.UnitHandle, opd, (int)right.AsInteger(), this, lst));
                        break;
                    case OpCode.NewFunV:
                        right = evalStack.Peek();
                        var lst1 = new FastList<DyObject[]>(captures);
                        lst1.Add(locals); //-opd
                        evalStack.Replace(new DyFunction(function.UnitHandle, opd, (int)right.AsInteger(), this, lst1) { Variadic = true });
                        break;
                    case OpCode.Call:
                        {
                            right = evalStack.Pop();
                            if (right.TypeId != StandardType.Function)
                            {
                                ctx.Error = Err.NotFunction(types[right.TypeId].TypeName);
                                ProcessError(ctx, function, ref offset);
                                break;
                            }

                            callFun = (DyFunction)right;

                            if (callFun.IsExternal) //Если так, то функция не внутренняя, а внешняя, например, написанная на C#
                                evalStack.Push(CallExternalFunction(opd, offset, evalStack, function, callFun, ctx));
                            else
                                goto FCALL;

                            if (ctx.Error != null)
                                ProcessError(ctx, function, ref offset, evalStack);
                        }
                        break;
                    case OpCode.Get:
                        left = evalStack.Pop();
                        right = evalStack.Peek();
                        evalStack.Replace(types[right.TypeId].GetOp(right, left, ctx));
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.Set:
                        left = evalStack.Pop();
                        right = evalStack.Pop();
                        if (opd >= StandardType.All.Count)
                            types[ctx.Assembly.Units[unit.ModuleHandles[opd & byte.MaxValue]].TypeHandles[opd >> 8]]
                                .SetOp(left.AsString(), right, ctx);
                        else
                            types[opd].SetOp(left.AsString(), right, ctx);
                        if (ctx.Error != null) ProcessError(ctx, function, ref offset, evalStack);
                        break;
                    case OpCode.RunMod:
                        ExecuteModule(unit.ModuleHandles[opd]);
                        evalStack.Push(new DyModule(Assembly.Units[opd], modules[opd]));
                        break;
                    case OpCode.Type:
                        evalStack.Replace(types[evalStack.Peek().TypeId]);
                        break;
                    case OpCode.Tag:
                        evalStack.Replace(new DyLabel(unit.IndexedStrings[opd].Value, evalStack.Peek()));
                        break;
                }
            }
            goto CYCLE;

        FCALL:
            layout = ctx.Assembly.Units[callFun.UnitHandle].Layouts[callFun.Handle];
            var newStack = new EvalStack(layout.StackSize);
            var max = opd > callFun.ParameterNumber ? callFun.ParameterNumber : opd;
            var tot = opd > callFun.ParameterNumber ? opd : callFun.ParameterNumber;

            //Надо выровнять либо стек, либо переданные аргументы
            for (var i = tot; i > 0; i--)
            {
                if (i > opd)
                    newStack.Push(DyNil.Instance);
                else
                {
                    var v = evalStack.Pop();
                    if (i <= max)
                        newStack.Push(v);
                }
            }

            ctx.CallStack.Push(new CallPoint(offset, evalStack, locals, function));
            function = callFun;
            evalStack = newStack;
            goto FPROLOG;

        FEPILOG:
            cp = ctx.CallStack.Pop();
            function = cp.Function;
            unit = Assembly.Units[function.UnitHandle];
            ops = unit.Ops;
            offset = cp.ReturnAddress;
            locals = cp.Locals;
            captures = function.Captures;
            evalStack = cp.Stack;
            evalStack.Push(right);
            goto CYCLE;
        }

        private void ProcessError(ExecutionContext ctx, DyFunction currentFunc, ref int offset, EvalStack evalStack = null)
        {
            if (evalStack != null)
                evalStack.PopVoid();

            throw CreateException(ctx.Error, offset, currentFunc.UnitHandle, ctx);
        }

        private DyObject CallExternalFunction(int opd, int offset, EvalStack evalStack, DyFunction caller, DyFunction fun, ExecutionContext ctx)
        {
            var max = opd > fun.ParameterNumber ? opd : fun.ParameterNumber;
            var arr = new DyObject[max];
            for (var i = opd - 1; i > -1; i--)
                arr[i] = evalStack.Pop();

            //Надо выровнять либо список параметров, либо переданные аргументы
            if (opd < fun.ParameterNumber)
                for (var i = opd; i < fun.ParameterNumber; i++)
                    arr[i] = DyNil.Instance;

            try
            {
                return fun.Call(ctx, arr);
            }
            catch (Exception ex)
            {
                throw CreateException(Err.ExternalFunctionFailure(fun.FunctionName, ex.Message),
                    offset - 1, caller.UnitHandle, ctx, ex);
            }
        }

        private static Stack<int> Dump(CallStack callStack)
        {
            var st = new Stack<int>();

            for (var i = 0; i < callStack.Count; i++)
            {
                var cm = callStack[i];
                st.Push(cm.ReturnAddress);
            }

            return st;
        }

        private DyCodeException CreateException(DyError err, int offset, int moduleHandle, ExecutionContext ctx, Exception ex = null)
        {
            var dump = Dump(ctx.CallStack);
            var deb = new Debug.DyDebugger(Assembly, moduleHandle);
            var cs = deb.BuildCallStack(offset, dump);
            return new DyCodeException(err, cs.File, cs.Line, cs.Column, cs, ex);
        }
    }
}
