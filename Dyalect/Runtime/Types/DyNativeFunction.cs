using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal class DyNativeFunction : DyFunction
    {
        internal FunSym? Sym;
        internal FastList<DyObject[]> Captures;
        internal DyObject[]? Locals;
        internal Stack<CatchMark> CatchMarks = null!;
        internal int PreviousOffset;
        internal int UnitId;
        internal int FunctionId;

        public override string FunctionName => Sym?.Name != null ? Sym.Name : DefaultName;

        public override bool IsExternal => false;

        internal override void Reset(ExecutionContext ctx)
        {
            Locals = null;
            PreviousOffset = ctx.RuntimeContext.Composition.Units[UnitId].Layouts[FunctionId].Size;
        }

        internal DyNativeFunction(FunSym? sym, int unitId, int funcId, FastList<DyObject[]> captures, int typeId, int varArgIndex) :
            base(typeId, sym?.Parameters ?? Array.Empty<Par>(), varArgIndex)
        {
            Sym = sym;
            UnitId = unitId;
            FunctionId = funcId;
            Captures = captures;
        }

        public static DyNativeFunction Create(FunSym sym, int unitId, int funcId, FastList<DyObject[]> captures, DyObject[] locals, int varArgIndex = -1)
        {
            var vars = new FastList<DyObject[]>(captures) { locals };
            return new DyNativeFunction(sym, unitId, funcId, vars, DyType.Function, varArgIndex);
        }

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg)
        {
            var captures = new FastList<DyObject[]>(Captures) { arg is DyCustomType ct ? ct.Locals : System.Array.Empty<DyObject>() };
            return new DyNativeFunction(Sym, UnitId, FunctionId, captures, DyType.Function, VarArgIndex)
            {
                Self = arg
            };
        }

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            if (args == null)
                args = Array.Empty<DyObject>();

            var locs = CreateLocals(ctx);
            var argCount = args.Length;

            if (VarArgIndex != -1 && args.Length >= VarArgIndex)
            {
                var arr = new DyObject[args.Length];
                locs[VarArgIndex] = new DyTuple(arr);

                for (var i = VarArgIndex; i < args.Length; i++)
                    arr[i] = args[i];

                argCount = VarArgIndex;
            }

            if (Parameters.Length != argCount && !ProcessArguments(ctx, locs, argCount))
                return DyNil.Instance;

            for (var i = 0; i < argCount; i++)
                locs[i] = args[i];

            ctx.CallStack.Push(Caller.External);
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        private bool ProcessArguments(ExecutionContext ctx, DyObject[] locs, int passed)
        {
            if (Parameters.Length < passed)
            {
                ctx.TooManyArguments(FunctionName, Parameters.Length, passed);
                return false;
            }
            else
            {
                for (var i = 2; i < Parameters.Length; i++)
                {
                    if (Parameters[i].Value is not null)
                        locs[i] = Parameters[i].Value!;
                    else
                    {
                        ctx.RequiredArgumentMissing(FunctionName, Parameters[i].Name);
                        return false;
                    }
                }
            }

            return true;
        }

        internal override DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var locs = CreateLocals(ctx);

            if (Parameters.Length != 2 && !ProcessArguments(ctx, locs, 2))
                return DyNil.Instance;

            locs[0] = left;
            locs[1] = right;
            ctx.CallStack.Push(Caller.External);
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override DyObject Call1(DyObject obj, ExecutionContext ctx)
        {
            var locs = CreateLocals(ctx);

            if (Parameters.Length != 1 && !ProcessArguments(ctx, locs, 2))
                return DyNil.Instance;

            locs[0] = obj;
            ctx.CallStack.Push(Caller.External);
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override DyObject Call0(ExecutionContext ctx)
        {
            var locs = CreateLocals(ctx);

            if (Parameters.Length != 0 && !ProcessArguments(ctx, locs, 2))
                return DyNil.Instance;

            ctx.CallStack.Push(Caller.External);
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override MemoryLayout GetLayout(ExecutionContext ctx) => ctx.RuntimeContext.Composition.Units[UnitId].Layouts[FunctionId];

        internal override DyObject[] CreateLocals(ExecutionContext ctx)
        {
            var size = GetLayout(ctx).Size;
            return size == 0 ? Array.Empty<DyObject>() : new DyObject[size];
        }

        internal override bool Equals(DyFunction func) => func is DyNativeFunction m
            && m.UnitId == UnitId && m.FunctionId == FunctionId;
    }
}
