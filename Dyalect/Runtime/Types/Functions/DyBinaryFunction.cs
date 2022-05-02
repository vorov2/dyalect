using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal sealed class DyBinaryFunction : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

    public DyBinaryFunction(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par par)
        : base(name, new Par[] { par }, -1) => this.fun = fun;

    private DyBinaryFunction(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par[] pars)
        : base(name, pars, -1) => this.fun = fun;

    internal override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0]);

    protected override DyFunction Clone(ExecutionContext ctx) => new DyBinaryFunction(FunctionName, fun, Parameters);

    public override object ToObject() => fun;

    protected override bool Equals(DyFunction func) => 
           func is DyBinaryFunction bin && ReferenceEquals(bin.fun, fun) && IsSameInstance(this, func);
}
