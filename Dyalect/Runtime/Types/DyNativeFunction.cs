using Dyalect.Compiler;
using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    internal class DyNativeFunction : DyFunction
    {
        internal FunSym Sym;
        internal FastList<DyObject[]> Captures;
        internal DyObject[] Locals;
        internal int PreviousOffset;
        internal int UnitId;
        internal int FunctionId;

        public override string FunctionName => Sym.Name;

        public override bool IsExternal => false;

        internal DyNativeFunction(FunSym sym, int unitId, int funcId, FastList<DyObject[]> captures, int typeId, int varArgIndex) : 
            base(typeId, sym?.Parameters ?? Statics.EmptyParameters, varArgIndex)
        {
            Sym = sym;
            UnitId = unitId;
            FunctionId = funcId;
            Captures = captures;
        }

        public static DyNativeFunction Create(FunSym sym, int unitId, int funcId, FastList<DyObject[]> captures, DyObject[] locals, int varArgIndex = -1)
        {
            var vars = new FastList<DyObject[]>(captures) { locals };
            return new DyNativeFunction(sym, unitId, funcId, vars, StandardType.Function, varArgIndex);
        }

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg)
        {
            return new DyNativeFunction(Sym, UnitId, FunctionId, Captures, StandardType.Function, VarArgIndex)
            {
                Self = arg
            };
        }

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            if (args == null)
                args = Statics.EmptyDyObjects;

            var locs = CreateLocals(ctx);
            var arr = default(DyObject[]);

            for (var i = 0; i < Parameters.Length; i++)
            {
                if (VarArgIndex != -1 && i >= VarArgIndex)
                {
                    if (arr == null)
                        arr = new DyObject[args.Length - i + 1];

                    if (i < args.Length)
                        arr[args.Length - i] = args[i];
                }

                locs[i] = i >= args.Length ? DyNil.Instance : args[i];
            }

            ctx.CallStack.Dup();
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var locs = CreateLocals(ctx);
            locs[0] = left;
            locs[1] = right;
            ctx.CallStack.Dup();
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override DyObject Call1(DyObject obj, ExecutionContext ctx)
        {
            var locs = CreateLocals(ctx);
            locs[0] = obj;
            ctx.CallStack.Dup();
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override DyObject Call0(ExecutionContext ctx)
        {
            var locs = CreateLocals(ctx);
            ctx.CallStack.Dup();
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override MemoryLayout GetLayout(ExecutionContext ctx) => ctx.Composition.Units[UnitId].Layouts[FunctionId];

        internal override DyObject[] CreateLocals(ExecutionContext ctx)
        {
            var size = GetLayout(ctx).Size;
            return size == 0 ? Statics.EmptyDyObjects : new DyObject[size];
        }
    }
}
