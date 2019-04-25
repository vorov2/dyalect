using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    internal class DyNativeFunction : DyFunction
    {
        internal const int VARIADIC = 0x01;

        protected DyMachine Machine { get; }

        internal FastList<DyObject[]> Captures { get; set; }

        internal DyObject[] Locals { get; set; }

        internal int PreviousOffset { get; set; }

        internal int UnitId { get; }

        internal int FunctionId { get; set; }

        internal byte Flags { get; set; }

        public bool IsVariadic => (Flags & VARIADIC) == VARIADIC;

        public DyNativeFunction(int unitId, int funcId, int pars, DyMachine vm, FastList<DyObject[]> captures, int typeId) : base(typeId, pars)
        {
            UnitId = unitId;
            FunctionId = funcId;
            ParameterNumber = pars;
            Machine = vm;
            Captures = captures;
        }

        public static DyNativeFunction Create(int unitId, int funcId, int pars, DyMachine vm, FastList<DyObject[]> captures, DyObject[] locals, bool variadic = false)
        {
            byte flags = 0;

            if (variadic)
                flags |= VARIADIC;

            var vars = new FastList<DyObject[]>(captures) { locals };
            return new DyNativeFunction(unitId, funcId, pars, vm, vars, StandardType.Function)
            {
                Flags = flags
            };
        }

        internal override DyFunction Clone(DyObject arg)
        {
            return new DyNativeFunction(UnitId, FunctionId, ParameterNumber, Machine, Captures, StandardType.Function)
            {
                Self = arg
            };
        }

        public override DyObject Call(params DyObject[] args) => throw new NotSupportedException();

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            if (Machine == null)
                return null;

            if (args == null)
                args = new DyObject[0];

            var opd = args.Length;
            var layout = Machine.Composition.Units[UnitId].Layouts[FunctionId];
            var newStack = new EvalStack(layout.StackSize);

            if (opd < ParameterNumber)
                for (var i = opd; i < ParameterNumber; i++)
                    newStack.Push(DyNil.Instance);

            var c = 0;
            DyObject[] arr = null;

            if (IsVariadic)
                arr = new DyObject[opd - ParameterNumber];

            for (var i = opd - 1; i > -1; i--)
            {
                if (++c > ParameterNumber)
                {
                    if (IsVariadic)
                        arr[opd - ParameterNumber] = args[i];
                }
                else
                    newStack.Push(args[i]);
            }

            if (IsVariadic)
                newStack.Push(DyTuple.Create(arr));

            return Machine.ExecuteWithData(this, newStack);
        }

        internal override DyObject Call3(DyObject arg1, DyObject arg2, DyObject arg3, ExecutionContext ctx)
        {
            var layout = Machine.Composition.Units[UnitId].Layouts[FunctionId];
            var newStack = new EvalStack(layout.StackSize);
            newStack.Push(arg3);
            newStack.Push(arg2);
            newStack.Push(arg1);
            return Machine.ExecuteWithData(this, newStack);
        }

        internal override DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var layout = Machine.Composition.Units[UnitId].Layouts[FunctionId];
            var newStack = new EvalStack(layout.StackSize);
            newStack.Push(right);
            newStack.Push(left);
            return Machine.ExecuteWithData(this, newStack);
        }

        internal override DyObject Call1(DyObject obj, ExecutionContext ctx)
        {
            var layout = Machine.Composition.Units[UnitId].Layouts[FunctionId];
            var newStack = new EvalStack(layout.StackSize);
            if (ParameterNumber > 1)
                newStack.Push(DyNil.Instance);
            newStack.Push(obj);
            return Machine.ExecuteWithData(this, newStack);
        }

        internal override DyObject Call0(ExecutionContext ctx)
        {
            var layout = Machine.Composition.Units[UnitId].Layouts[FunctionId];
            return Machine.ExecuteWithData(this, new EvalStack(layout.StackSize));
        }

        protected override string GetFunctionName() => GetFunSym()?.Name ?? DefaultName;

        private FunSym GetFunSym()
        {
            var frame = Machine?.Composition.Units[UnitId];
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

        protected override string[] GetCustomParameterNames() => GetFunSym()?.Parameters;
    }
}
