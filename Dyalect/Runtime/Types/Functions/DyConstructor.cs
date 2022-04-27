using Dyalect.Compiler;
using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types;

internal sealed class DyConstructor<P1> : DyForeignFunction
    where P1 : DyObject
{
    private readonly Func<ExecutionContext, P1, DyObject> fun;

    public DyConstructor(string name, Func<ExecutionContext, P1, DyObject> fun, Par par)
        : base(name, new[] { par }) => (this.fun, Attr) = (fun, FunAttr.Vari);

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Cast<P1>(0, args)!);

    protected override DyFunction Clone(ExecutionContext ctx) => this;
}