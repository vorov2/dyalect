using Dyalect.Compiler;
using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

internal sealed class DyConstructor : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyTuple, DyObject> fun;

    public DyConstructor(string name, Func<ExecutionContext, DyTuple, DyObject> fun, Par par)
        : base(name, new[] { par }) => (this.fun, Attr) = (fun, FunAttr.Vari);

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, (DyTuple)args[0]);

    protected override DyFunction Clone(ExecutionContext ctx) => this;
}