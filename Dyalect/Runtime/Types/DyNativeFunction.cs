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
        internal int VarArgIndex;

        public override string FunctionName => Sym.Name;

        public DyNativeFunction(FunSym sym, int unitId, int funcId, FastList<DyObject[]> captures, int typeId) : base(typeId, sym?.Parameters ?? Statics.EmptyParameters)
        {
            Sym = sym;
            UnitId = unitId;
            FunctionId = funcId;
            Captures = captures;
        }

        public static DyNativeFunction Create(FunSym sym, int unitId, int funcId, FastList<DyObject[]> captures, DyObject[] locals, int varArgsIndex = -1)
        {
            var vars = new FastList<DyObject[]>(captures) { locals };
            return new DyNativeFunction(sym, unitId, funcId, vars, StandardType.Function)
            {
                VarArgIndex = varArgsIndex
            };
        }

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg)
        {
            return new DyNativeFunction(Sym, UnitId, FunctionId, Captures, StandardType.Function)
            {
                Self = arg
            };
        }

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            if (args == null)
                args = Statics.EmptyDyObjects;

            var layout = ctx.Composition.Units[UnitId].Layouts[FunctionId];
            var locs = new DyObject[layout.Size];

            for (var i = 0; i < Parameters.Length; i++)
                locs[i] = i >= args.Length ? DyNil.Instance : args[i];

            if (VarArgIndex >= 0)
            {
                var arr = new DyObject[args.Length - Parameters.Length];

                for (var i = Parameters.Length; i < args.Length; i++)
                    arr[i - Parameters.Length] = args[i];

                locs[locs.Length - 1] = DyTuple.Create(arr);
            }

            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var locs = new DyObject[ctx.Composition.Units[UnitId].Layouts[FunctionId].Size];
            locs[0] = left;
            locs[1] = right;
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override DyObject Call1(DyObject obj, ExecutionContext ctx)
        {
            var locs = new DyObject[ctx.Composition.Units[UnitId].Layouts[FunctionId].Size];
            locs[0] = obj;
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }

        internal override DyObject Call0(ExecutionContext ctx)
        {
            var size = ctx.Composition.Units[UnitId].Layouts[FunctionId].Size;
            var locs = size == 0 ? Statics.EmptyDyObjects : new DyObject[size];
            return DyMachine.ExecuteWithData(this, locs, ctx);
        }
    }
}
