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
