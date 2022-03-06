namespace Dyalect.Runtime.Types
{
    internal sealed class DyNativeIteratorFunction : DyNativeFunction
    {
        public override string FunctionName => "iter";

        public DyNativeIteratorFunction(int unitId, int funcId, FastList<DyObject[]> captures)
            : base(null, unitId, funcId, captures, -1) { }

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg) =>
            new DyNativeIteratorFunction(UnitId, FunctionId, Captures) { Self = arg };
    }
}
