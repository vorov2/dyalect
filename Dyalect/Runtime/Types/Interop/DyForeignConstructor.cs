using Dyalect.Debug;

namespace Dyalect.Runtime.Types;

internal sealed class DyForeignConstructor : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

    public DyForeignConstructor(Func<ExecutionContext, DyObject, DyObject, DyObject> fun)
        : base("new", new Par[] { new Par("values", ParKind.VarArg) }, 0) => this.fun = fun;

    private DyForeignConstructor(Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par[] pars) : base("new", pars, 0) => this.fun = fun;

    protected override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0]);

    protected override DyFunction Clone(ExecutionContext ctx) => new DyForeignConstructor(fun, Parameters);

    protected override bool Equals(DyFunction func) => func is DyForeignConstructor c && c.fun.Equals(fun);
}
