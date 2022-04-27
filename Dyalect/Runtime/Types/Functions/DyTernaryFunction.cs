using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

internal sealed class DyTernaryFunction : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun;

    public DyTernaryFunction(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par par1, Par par2)
        : base(name, new Par[] { par1, par2 }, -1) => this.fun = fun;

    private DyTernaryFunction(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars)
        : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0], args[1]);

    protected override DyFunction Clone(ExecutionContext ctx) => new DyTernaryFunction(FunctionName, fun, Parameters);

    internal override bool Equals(DyFunction func) => func is DyTernaryFunction ter && ReferenceEquals(ter.fun, fun)
        && (ReferenceEquals(ter.Self, Self) || (ter.Self is not null && ter.Self.Equals(Self)));
}
