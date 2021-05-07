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

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
        {
            var size = GetLayout(ctx).Size;
            DyObject[] locals;

            if (size == args.Length)
                locals = args;
            else if (size > 0)
            {
                locals = CreateLocals(ctx);

                for (var i = 0; i < args.Length; i++)
                    locals[i] = args[i];
            }
            else
                locals = Array.Empty<DyObject>();

            ctx.CallStack.Push(Caller.External);
            return DyMachine.ExecuteWithData(this, locals, ctx);
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
