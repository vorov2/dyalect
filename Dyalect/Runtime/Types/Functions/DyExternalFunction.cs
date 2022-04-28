using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

public sealed class DyExternalFunction : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject?, DyObject[], DyObject> func;

    public DyExternalFunction(string name, Func<ExecutionContext, DyObject?, DyObject[], DyObject> func, params Par[] pars)
        : base(name, pars)  => this.func = func;

    public override DyObject Clone() => new DyExternalFunction(FunctionName, func, Parameters);

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) =>
        func(ctx, Self, args);

    public override object ToObject() => func;

    internal override bool Equals(DyFunction func) =>
           FunctionName == func.FunctionName
        && func is DyExternalFunction fn && fn.func.Equals(func)
        && IsSameInstance(this, func);
}
