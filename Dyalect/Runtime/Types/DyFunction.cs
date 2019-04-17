using Dyalect.Debug;
using System;
using System.Reflection;

namespace Dyalect.Runtime.Types
{
    public class DyFunction : DyObject
    {
        internal delegate object CallHandler(params object[] args);

        internal const int EXT_HANDLE = -1;
        internal const int OVL_HANDLE = -2;
        private const string DEF_NAME = "<func>";
        private readonly DyMachine vm;
        internal FastList<DyObject[]> Captures;

        internal DyFunction(int moduleHandle, int handle, int pars, DyMachine vm, FastList<DyObject[]> captures) : base(StandardType.Function)
        {
            UnitHandle = moduleHandle;
            Handle = handle;
            this.ParameterNumber = pars;
            this.vm = vm;
            this.Captures = captures;
        }

        internal DyFunction(int handle, int typeCode) : base(typeCode)
        {
            Handle = handle;
        }

        internal DyFunction(int typeCode) : this(EXT_HANDLE, typeCode)
        {

        }

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(this, obj);

        internal bool IsExternal => Handle == EXT_HANDLE;

        internal bool IsOverloaded => Handle == OVL_HANDLE;

        internal int UnitHandle { get; }

        internal int Handle { get; set; }

        public int ParameterNumber { get; protected set; }

        public bool Variadic { get; internal set; }

        public override object AsObject() => (CallHandler)Call;

        internal DyObject Self { get; set; }

        internal DyFunction Clone(DyObject arg)
        {
            return new DyFunction(UnitHandle, Handle, ParameterNumber, vm, Captures)
            {
                Self = arg
            };
        }

        public object Call(params object[] args)
        {
            var pars = new DyObject[args.Length];

            for (var i = 0; i < args.Length; i++)
            {
                pars[i] = TypeConverter.ConvertFrom(args[i], vm.ExecutionContext);
                vm.ExecutionContext.ThrowIf();
            }

            return Call(pars);
        }

        public DyObject Call(params DyObject[] args)
        {
            var callStack = new CallStack();
            var ctx = new ExecutionContext(callStack, vm.Assembly);
            var retval = Call(ctx, args);
            ctx.ThrowIf();
            return retval;
        }

        public virtual DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            if (vm == null)
                return null;

            if (args == null)
                args = new DyObject[0];

            var opd = args.Length;
            var layout = vm.Assembly.Units[UnitHandle].Layouts[Handle];
            var newStack = new EvalStack(layout.StackSize);

            //Здесь нам нужно выровнять либо стек либо параметры функции
            if (opd < ParameterNumber)
                for (var i = opd; i < ParameterNumber; i++)
                    newStack.Push(DyNil.Instance);

            var c = 0;
            DyObject[] arr = null;

            if (Variadic)
                arr = new DyObject[opd - ParameterNumber];

            for (var i = opd - 1; i > -1; i--)
            {
                if (++c > ParameterNumber)
                {
                    if (Variadic)
                        arr[opd - ParameterNumber] = args[i];
                }
                else
                    newStack.Push(args[i]);
            }

            //TODO: Variadic
            //if (Variadic) 
            //    newStack.Push(new DysTuple(arr));

            return vm.ExecuteWithData(this, newStack);
        }

        internal DyObject Call3(DyObject arg1, DyObject arg2, DyObject arg3, ExecutionContext ctx)
        {
            if (vm == null)
                return null;

            var layout = vm.Assembly.Units[UnitHandle].Layouts[Handle];
            var newStack = new EvalStack(layout.StackSize);
            newStack.Push(arg3);
            newStack.Push(arg2);
            newStack.Push(arg1);
            return vm.ExecuteWithData(this, newStack);
        }

        internal DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (vm == null)
                return null;

            var layout = vm.Assembly.Units[UnitHandle].Layouts[Handle];
            var newStack = new EvalStack(layout.StackSize);
            newStack.Push(right);
            newStack.Push(left);
            return vm.ExecuteWithData(this, newStack);
        }

        internal DyObject Call1(DyObject obj, ExecutionContext ctx)
        {
            if (vm == null)
                return null;

            var layout = vm.Assembly.Units[UnitHandle].Layouts[Handle];
            var newStack = new EvalStack(layout.StackSize);
            if (ParameterNumber > 1)
                newStack.Push(DyNil.Instance);
            newStack.Push(obj);
            return vm.ExecuteWithData(this, newStack);
        }

        protected virtual string GetFunctionName() => GetFunSym()?.Name ?? DEF_NAME;

        public string[] GetParameterNames()
        {
            var dynParameters = this.GetCustomParameterNames();

            if (dynParameters != null)
                return dynParameters;

            var fs = this.GetFunSym();

            if (fs != null)
                return fs.Parameters;

            var arr = new string[this.ParameterNumber];

            for (var i = 0; i < this.ParameterNumber; i++)
                arr[i] = "p" + i;

            return arr;
        }

        protected virtual string[] GetCustomParameterNames() => null;

        private FunSym GetFunSym()
        {
            var frame = vm?.Assembly.Units[UnitHandle];
            var syms = frame != null ? frame.Symbols : null;

            if (syms != null)
            {
                var dr = new DebugReader(syms);
                var fs = dr.GetFunSymByHandle(Handle);

                if (fs != null)
                    return fs;
            }

            return null;
        }

        public override string ToString()
        {
            var nm = GetFunctionName();
            var pars = GetParameterNames();
            return nm
                + "("
                + string.Join(",", pars)
                + (Variadic ? "..." : "")
                + ")";
        }

        private string _functionName;
        public string FunctionName
        {
            get
            {
                if (_functionName == null)
                    _functionName = GetFunctionName();

                return _functionName ?? DEF_NAME;
            }
        }

        #region Create
        internal static DyFunction Create(int moduleHandle, int handle, int paramNumber, DyMachine vm, FastList<DyObject[]> captures, bool variadic)
        {
            return new DyFunction(moduleHandle, handle, paramNumber, vm, captures)
            {
                Variadic = variadic
            };
        }

        internal static DyFunction Create(int moduleHandle, int handle, int paramNumber, DyMachine vm, FastList<DyObject[]> captures)
        {
            return new DyFunction(moduleHandle, handle, paramNumber, vm, captures);
        }

        internal static DyFunction Create(Func<ExecutionContext, DyObject[], DyObject> fun, string name)
        {
            return Create(fun, -1, name, new CallAdapter.ArgCtx(fun));
        }

        public static DyFunction Create(Func<DyObject> fun, string name = null)
        {
            return Create(fun, 0, name, new CallAdapter.Arg0(fun));
        }

        public static DyFunction Create(Func<DyObject, DyObject> fun, string name = null)
        {
            return Create(fun, 1, name, new CallAdapter.Arg1(fun));
        }

        public static DyFunction Create(Func<DyObject, DyObject, DyObject> fun, string name = null)
        {
            return Create(fun, 2, name, new CallAdapter.Arg2(fun));
        }

        public static DyFunction Create(Func<DyObject, DyObject, DyObject, DyObject> fun, string name = null)
        {
            return Create(fun, 3, name, new CallAdapter.Arg3(fun));
        }

        public static DyFunction Create(Func<DyObject, DyObject, DyObject, DyObject, DyObject> fun, string name = null)
        {
            return Create(fun, 4, name, new CallAdapter.Arg4(fun));
        }

        public static DyFunction Create(Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, string name = null)
        {
            return Create(fun, 5, name, new CallAdapter.Arg5(fun));
        }

        public static DyFunction Create(Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, string name = null)
        {
            return Create(fun, 6, name, new CallAdapter.Arg6(fun));
        }

        public static DyFunction Create(Func<DyObject[], DyObject> fun, string name = null)
        {
            return Create(fun, -1, name, new CallAdapter.ArgAny(fun));
        }

        public static DyFunction Create<T1>(Func<T1> fun, string name = null)
        {
            return Create(fun, 0, name, new CallAdapter.Arg0<T1>(fun));
        }

        public static DyFunction Create<T1, T2>(Func<T1, T2> fun, string name = null)
        {
            return Create(fun, 1, name, new CallAdapter.Arg1<T1, T2>(fun));
        }

        public static DyFunction Create<T1, T2, T3>(Func<T1, T2, T3> fun, string name = null)
        {
            return Create(fun, 2, name, new CallAdapter.Arg2<T1, T2, T3>(fun));
        }

        public static DyFunction Create<T1, T2, T3, T4>(Func<T1, T2, T3, T4> fun, string name = null)
        {
            return Create(fun, 3, name, new CallAdapter.Arg3<T1, T2, T3, T4>(fun));
        }

        public static DyFunction Create<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> fun, string name = null)
        {
            return Create(fun, 4, name, new CallAdapter.Arg4<T1, T2, T3, T4, T5>(fun));
        }

        public static DyFunction Create<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> fun, string name = null)
        {
            return Create(fun, 5, name, new CallAdapter.Arg5<T1, T2, T3, T4, T5, T6>(fun));
        }

        public static DyFunction Create<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> fun, string name = null)
        {
            return Create(fun, 6, name, new CallAdapter.Arg6<T1, T2, T3, T4, T5, T6, T7>(fun));
        }

        public static DyFunction Create<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8> fun, string name = null)
        {
            return Create(fun, 7, name, new CallAdapter.Arg7<T1, T2, T3, T4, T5, T6, T7, T8>(fun));
        }

        public static DyFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9> fun, string name = null)
        {
            return Create(fun, 8, name, new CallAdapter.Arg8<T1, T2, T3, T4, T5, T6, T7, T8, T9>(fun));
        }

        public static DyFunction Create<T1, T2>(Func<T1[], T2> fun, string name = null)
        {
            return Create(fun, -1, name, new CallAdapter.ArgAny<T1, T2>(fun));
        }

        private static DyFunction Create(Delegate fun, int args, string name, CallAdapter adapter)
        {
            name = name ?? fun.GetMethodInfo().Name;
            var ret = new DyDelegateFunction(name, args < 0 ? 0 : args, adapter);

            if (args < 0)
                ret.Variadic = true;

            return ret;
        }
        #endregion
    }

    internal sealed class DyFunctionTypeInfo : DyTypeInfo
    {
        public static readonly DyFunctionTypeInfo Instance = new DyFunctionTypeInfo();

        private DyFunctionTypeInfo() : base(StandardType.Function)
        {

        }

        public override string TypeName => StandardType.FunctionName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(((DyFunction)arg).ToString());
    }
}
