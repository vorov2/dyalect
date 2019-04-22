using System;
using System.Reflection;

namespace Dyalect.Runtime.Types
{
    public abstract class DyFunction : DyObject
    {
        internal const string DefaultName = "<func>";

        public int ParameterNumber { get; protected set; }

        internal DyObject Self { get; set; }

        protected DyFunction(int typeId, int pars) : base(typeId)
        {
            ParameterNumber = pars;
        }

        public override object ToObject() => (Func<ExecutionContext, DyObject[], DyObject>)Call;

        internal abstract DyFunction Clone(DyObject arg);

        public virtual DyObject Call(params DyObject[] args) => Call(null, args);

        public virtual DyObject Call(ExecutionContext ctx, params DyObject[] args) => Call(args);
        
        internal virtual DyObject Call3(DyObject arg1, DyObject arg2, DyObject arg3, ExecutionContext ctx) => Call(arg1, arg2, arg3);

        internal virtual DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx) =>  Call(left, right);

        internal virtual DyObject Call1(DyObject obj, ExecutionContext ctx) => Call(obj);

        protected abstract string GetFunctionName();

        public string[] GetParameterNames()
        {
            var dynParameters = GetCustomParameterNames();

            if (dynParameters != null)
                return dynParameters;

            var arr = new string[ParameterNumber];

            for (var i = 0; i < ParameterNumber; i++)
                arr[i] = "p" + i;

            return arr;
        }

        protected virtual string[] GetCustomParameterNames() => null;

        public override string ToString() => 
            $"{GetFunctionName()}({string.Join(",", GetParameterNames())})";

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
