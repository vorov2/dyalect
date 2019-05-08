using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    internal class DyNativeFunction : DyFunction
    {
        internal const int VARIADIC = 0x01;

        internal FastList<DyObject[]> Captures { get; set; }

        internal DyObject[] Locals { get; set; }

        internal int PreviousOffset { get; set; }

        internal int UnitId { get; }

        internal int FunctionId { get; set; }

        internal byte Flags { get; set; }

        public bool IsVariadic => (Flags & VARIADIC) == VARIADIC;

        public DyNativeFunction(int unitId, int funcId, int pars, FastList<DyObject[]> captures, int typeId) : base(typeId, pars)
        {
            UnitId = unitId;
            FunctionId = funcId;
            ParameterNumber = pars;
            Captures = captures;
        }

        public static DyNativeFunction Create(int unitId, int funcId, int pars, FastList<DyObject[]> captures, DyObject[] locals, bool variadic = false)
        {
            byte flags = 0;

            if (variadic)
                flags |= VARIADIC;

            var vars = new FastList<DyObject[]>(captures) { locals };
            return new DyNativeFunction(unitId, funcId, pars, vars, StandardType.Function)
            {
                Flags = flags
            };
        }

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg)
        {
            return new DyNativeFunction(UnitId, FunctionId, ParameterNumber, Captures, StandardType.Function)
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

            for (var i = 0; i < ParameterNumber; i++)
                locs[i] = i >= args.Length ? DyNil.Instance : args[i];

            if (IsVariadic)
            {
                var arr = new DyObject[args.Length - ParameterNumber];

                for (var i = ParameterNumber; i < args.Length; i++)
                    arr[i - ParameterNumber] = args[i];

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

        protected override string GetCustomFunctionName(ExecutionContext ctx) => GetFunSym(ctx)?.Name ?? DefaultName;

        private FunSym GetFunSym(ExecutionContext ctx)
        {
            var frame = ctx?.Composition.Units[UnitId];
            var syms = frame != null ? frame.Symbols : null;

            if (syms != null)
            {
                var dr = new DebugReader(syms);
                var fs = dr.GetFunSymByHandle(FunctionId);

                if (fs != null)
                    return fs;
            }

            return null;
        }

        protected override FunctionParameter[] GetCustomParameterNames(ExecutionContext ctx) => GetFunSym(ctx)?.Parameters;
    }
}
