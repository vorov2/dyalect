using Dyalect.Compiler;
using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

internal sealed class StaticProperty : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject> fun;

    public StaticProperty(string name, Func<ExecutionContext, DyObject> fun) : base(name, Array.Empty<Par>(), -1)
    {
        this.fun = fun;
        Attr |= FunAttr.Auto;
    }

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx);

    internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg) => fun(ctx);

    protected override DyFunction Clone(ExecutionContext ctx) => this;
}

internal sealed class InstanceProperty<P1> : DyForeignFunction where P1 : DyObject
{
    private readonly Func<ExecutionContext, P1?, DyObject> fun;

    public InstanceProperty(string name, Func<ExecutionContext, P1?, DyObject> fun) : base(name, Array.Empty<Par>(), -1)
    {
        this.fun = fun;
        Attr |= FunAttr.Auto;
    }

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, (P1)Self!);

    internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg) => fun(ctx, (P1)arg);

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceProperty<P1>(FunctionName, fun);
}

internal sealed class InstancePropertyNoContext<P1> : DyForeignFunction where P1 : DyObject
{
    private readonly Func<P1?, DyObject> fun;

    public InstancePropertyNoContext(string name, Func<P1?, DyObject> fun) : base(name, Array.Empty<Par>(), -1)
    {
        this.fun = fun;
        Attr |= FunAttr.Auto;
    }

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun((P1)Self!);

    internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg) => fun( (P1)arg);

    protected override DyFunction Clone(ExecutionContext ctx) => new InstancePropertyNoContext<P1>(FunctionName, fun);
}