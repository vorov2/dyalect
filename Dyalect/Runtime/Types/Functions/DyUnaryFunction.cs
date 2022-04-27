using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

internal class DyUnaryFunction : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject, DyObject> fun;

    public DyUnaryFunction(string name, Func<ExecutionContext, DyObject, DyObject> fun)
        : base(name, Array.Empty<Par>(), -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!);

    protected override DyFunction Clone(ExecutionContext ctx) => new DyUnaryFunction(FunctionName, fun);

    internal override bool Equals(DyFunction func) => func is DyUnaryFunction bin && ReferenceEquals(bin.fun, fun)
        && (ReferenceEquals(bin.Self, Self) || (bin.Self is not null && bin.Self.Equals(Self)));
}