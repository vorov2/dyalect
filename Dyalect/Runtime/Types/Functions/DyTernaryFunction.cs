using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal sealed class DyTernaryFunction : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun;

    public DyTernaryFunction(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par par1, Par par2)
        : base(name, new Par[] { par1, par2 }, -1) => this.fun = fun;

    private DyTernaryFunction(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars)
        : base(name, pars, -1) => this.fun = fun;

    protected override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0], args[1]);

    protected override DyFunction Clone(ExecutionContext ctx) => new DyTernaryFunction(FunctionName, fun, Parameters);

    public override object ToObject() => fun;

    protected override bool Equals(DyFunction func) =>
           func is DyTernaryFunction ter && ReferenceEquals(ter.fun, fun)
        && IsSameInstance(this, func);
}
