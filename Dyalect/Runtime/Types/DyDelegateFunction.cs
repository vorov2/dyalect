namespace Dyalect.Runtime.Types
{
    internal sealed class DyDelegateFunction : DyFunction
    {
        private readonly string name;
        private readonly CallAdapter adapter;

        internal DyDelegateFunction(string name, int pars, CallAdapter adapter) : base(0, EXT_HANDLE, pars, null, null)
        {
            this.name = name;
            this.adapter = adapter;
        }

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => adapter.Call(ctx, args);

        protected override string GetFunctionName() => name;
    }
}