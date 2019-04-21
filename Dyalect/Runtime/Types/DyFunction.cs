using System;
using System.Reflection;

namespace Dyalect.Runtime.Types
{
    public abstract class DyFunction : DyObject
    {
        internal const int VARIADIC = 0x02;
        internal const string DefaultName = "<func>";

        public int ParameterNumber { get; protected set; }

        internal DyObject Self { get; set; }

        protected DyFunction(int typeId, int pars) : base(typeId)
        {
            ParameterNumber = pars;
        }

        public override object ToObject() => (Func<DyObject[], DyObject>)Call;

        internal abstract DyFunction Clone(DyObject arg);

        public virtual DyObject Call(params DyObject[] args) => Call(null, args);

        public virtual DyObject Call(ExecutionContext ctx, params DyObject[] args) => Call(args);
        
        internal virtual DyObject Call3(DyObject arg1, DyObject arg2, DyObject arg3, ExecutionContext ctx)
        {
            return Call(arg1, arg2, arg3);
        }

        internal virtual DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx)
        {
            return Call(left, right);
        }

        internal virtual DyObject Call1(DyObject obj, ExecutionContext ctx)
        {
            return Call(obj);
        }

        protected abstract string GetFunctionName();

        public string[] GetParameterNames()
        {
            var dynParameters = this.GetCustomParameterNames();

            if (dynParameters != null)
                return dynParameters;

            var arr = new string[this.ParameterNumber];

            for (var i = 0; i < this.ParameterNumber; i++)
                arr[i] = "p" + i;

            return arr;
        }

        protected virtual string[] GetCustomParameterNames() => null;

        public override string ToString()
        {
            var nm = GetFunctionName();
            var pars = GetParameterNames();
            return nm
                + "("
                + string.Join(",", pars)
                + ")";
        }

        private string _functionName;
        public string FunctionName
        {
            get
            {
                if (_functionName == null)
                    _functionName = GetFunctionName();

                return _functionName ?? DefaultName;
            }
        }

        #region Create
        internal static DyNativeFunction Create(int unitId, int funcId, int pars, DyMachine vm, FastList<DyObject[]> captures, DyObject[] locals, bool variadic = false)
        {
            byte flags = 0;

            if (variadic)
                flags |= VARIADIC;

            var vars = new FastList<DyObject[]>(captures);
            vars.Add(locals);
            return new DyNativeFunction(unitId, funcId, pars, vm, vars, StandardType.Function)
            {
                Flags = flags
            };
        }

        internal static DyFunction Create(Func<ExecutionContext, DyObject[], DyObject> fun, string name)
        {
            return Create(fun, -1, name, new CallAdapter.ArgCtx(fun));
        }

        public static DyFunction Create(Func<DyObject, ExecutionContext, DyObject> fun, string name = null)
        {
            return Create(fun, 0, name, new CallAdapter.ArgUnary(fun));
        }

        public static DyFunction Create(Func<DyObject, DyObject, ExecutionContext, DyObject> fun, string name = null)
        {
            return Create(fun, 0, name, new CallAdapter.ArgBinary(fun));
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

        public static DyFunction Create<T1, T2>(Func<ExecutionContext, T1[], T2> fun, string name = null)
        {
            return Create(fun, -1, name, new CallAdapter.ArgAnyCtx<T1, T2>(fun));
        }

        private static DyFunction Create(Delegate fun, int args, string name, CallAdapter adapter)
        {
            name = name ?? fun.GetMethodInfo().Name;
            var ret = new DyDelegateFunction(name, args < 0 ? 0 : args, adapter);

            //if (args < 0)
            //    ret.Flags = VARIADIC;

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

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(((DyFunction)arg).ToString());
    }
}
