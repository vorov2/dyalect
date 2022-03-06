namespace Dyalect.Runtime.Types
{
    internal sealed class DyNativeIteratorFunction : DyNativeFunction
    {
        public override string FunctionName => "iter";

        public DyNativeIteratorFunction(DyTypeInfo typeInfo, int unitId, int funcId, FastList<DyObject[]> captures)
            : base(typeInfo, null, unitId, funcId, captures, -1) { }

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg) =>
            new DyNativeIteratorFunction(ctx.RuntimeContext.Function, UnitId, FunctionId, Captures) { Self = arg };
    }
}
