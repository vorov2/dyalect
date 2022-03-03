using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal class DyNativeFunction : DyFunction
    {
        private static readonly Stack<CatchMark> emptyMarks = new();

        private readonly FunSym? sym;
        internal readonly FastList<DyObject[]> Captures;
        internal DyObject[]? Locals;
        internal Stack<CatchMark> CatchMarks = null!;
        internal int PreviousOffset;
        internal readonly int UnitId;
        internal readonly int FunctionId;

        public override string FunctionName => sym?.Name != null ? sym.Name : DefaultName;

        public override bool IsExternal => false;

        internal override void Reset(ExecutionContext ctx)
        {
            Locals = null;
            PreviousOffset = ctx.RuntimeContext.Composition.Units[UnitId].Layouts[FunctionId].Size;
        }

        internal DyNativeFunction(FunSym? sym, int unitId, int funcId, FastList<DyObject[]> captures, int typeId, int varArgIndex) :
            base(typeId, sym?.Parameters ?? Array.Empty<Par>(), varArgIndex)
        {
            this.sym = sym;
            UnitId = unitId;
            FunctionId = funcId;
            Captures = captures;
        }

        public static DyNativeFunction Create(FunSym sym, int unitId, int funcId, FastList<DyObject[]> captures, DyObject[] locals, int varArgIndex = -1)
        {
            var vars = new FastList<DyObject[]>(captures) { locals };
            return new(sym, unitId, funcId, vars, DyType.Function, varArgIndex);
        }

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg) =>
            new DyNativeFunction(sym, UnitId, FunctionId, Captures, DyType.Function, VarArgIndex)
            {
                Self = arg
            };

        internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg)
        {
            if (Auto)
            {
                try
                {
                    var size = GetLayout(ctx).Size;
                    var locals = size == 0 ? Array.Empty<DyObject>() : new DyObject[size];
                    ctx.CallStack.Push(Caller.External);
                    return DyMachine.ExecuteWithData((DyNativeFunction)BindToInstance(ctx, arg), locals, ctx);
                }
                catch (DyCodeException ex)
                {
                    ctx.Error = ex.Error;
                    return DyNil.Instance;
                }
            }

            return BindToInstance(ctx, arg);
        }

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
        {
            try
            {
                var size = GetLayout(ctx).Size;
                DyObject[] locals;

                if (size == args.Length)
                    locals = args;
                else
                {
                    locals = size == 0 ? Array.Empty<DyObject>() : new DyObject[size];

                    for (var i = 0; i < args.Length; i++)
                        locals[i] = args[i];
                }

                ctx.CallStack.Push(Caller.External);
                return DyMachine.ExecuteWithData(this, locals, ctx);
            }
            catch (DyCodeException ex)
            {
                ctx.Error = ex.Error;
                return DyNil.Instance;
            }
        }

        internal override DyObject InternalCall(ExecutionContext ctx)
        {
            try
            {
                ctx.CallStack.Push(Caller.External);
                return DyMachine.ExecuteWithData(this, CreateLocals(ctx), ctx);
            }
            catch (DyCodeException ex)
            {
                ctx.Error = ex.Error;
                return DyNil.Instance;
            }
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
