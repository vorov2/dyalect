using System;

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

        internal abstract DyFunction Clone(ExecutionContext ctx, DyObject arg);

        public abstract DyObject Call(ExecutionContext ctx, params DyObject[] args);

        internal virtual DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx) =>  Call(ctx, left, right);

        internal virtual DyObject Call1(DyObject obj, ExecutionContext ctx) => Call(ctx, obj);

        internal virtual DyObject Call0(ExecutionContext ctx) => Call(ctx, Statics.EmptyDyObjects);

        protected abstract string GetCustomFunctionName(ExecutionContext ctx);

        public string[] GetParameterNames(ExecutionContext ctx)
        {
            var dynParameters = GetCustomParameterNames(ctx);

            if (dynParameters != null)
                return dynParameters;

            var arr = new string[ParameterNumber];

            for (var i = 0; i < ParameterNumber; i++)
                arr[i] = "p" + i;

            return arr;
        }

        protected virtual string[] GetCustomParameterNames(ExecutionContext ctx) => null;

        public string ToString(ExecutionContext ctx) => 
            $"{GetFunctionName(ctx)}({string.Join(",", GetParameterNames(ctx))})";

        private string _functionName;
        public string GetFunctionName(ExecutionContext ctx)
        {
            if (_functionName == null)
                _functionName = GetCustomFunctionName(ctx);

            return _functionName ?? DefaultName;
        }
    }

    internal sealed class DyFunctionTypeInfo : DyTypeInfo
    {
        public static readonly DyFunctionTypeInfo Instance = new DyFunctionTypeInfo();

        private DyFunctionTypeInfo() : base(StandardType.Function, false)
        {

        }

        public override string TypeName => StandardType.FunctionName;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(((DyFunction)arg).ToString());
    }
}
