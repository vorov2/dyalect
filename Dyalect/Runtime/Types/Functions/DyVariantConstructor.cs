using Dyalect.Compiler;
using Dyalect.Debug;

namespace Dyalect.Runtime.Types;

internal sealed class DyVariantConstructor : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyTuple, DyObject> fun;

    public DyVariantConstructor(string name, Func<ExecutionContext, DyTuple, DyObject> fun, Par par)
        : base(name, new[] { par }) => (this.fun, Attr) = (fun, FunAttr.Variadic);

    protected override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args) => fun(ctx, (DyTuple)args[0]);

    protected override DyFunction Clone(ExecutionContext ctx) => this;

    public override object ToObject() => fun;

    protected override bool Equals(DyFunction func) => func is DyVariantConstructor c && c.fun.Equals(fun);
}