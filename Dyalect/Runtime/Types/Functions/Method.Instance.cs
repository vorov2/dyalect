using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

internal sealed class InstanceMethod<P1> : DyForeignFunction
    where P1 : DyObject
{
    private readonly Func<ExecutionContext, P1, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1, DyObject> fun) : base(name, Array.Empty<Par>()) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!);
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1>(FunctionName, fun);
}

internal sealed class InstanceMethod<P1, P2> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1?, P2?, DyObject> fun, params Par[] pars) : base(name, pars) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!, Cast<P2>(0, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1, P2>(FunctionName, fun, Parameters);
}

internal sealed class InstanceMethod<P1, P2, P3> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1?, P2?, P3?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!, Cast<P2>(0, args), Cast<P3>(1, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1, P2, P3>(FunctionName, fun, Parameters);
}

internal sealed class InstanceMethod<P1, P2, P3, P4> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!, Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1, P2, P3, P4>(FunctionName, fun, Parameters);
}

internal sealed class InstanceMethod<P1, P2, P3, P4, P5> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
    where P5 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!, Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1, P2, P3, P4, P5>(FunctionName, fun, Parameters);
}

internal sealed class InstanceMethod<P1, P2, P3, P4, P5, P6> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
    where P5 : DyObject
    where P6 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!, Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args), Cast<P6>(4, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1, P2, P3, P4, P5, P6>(FunctionName, fun, Parameters);
}

internal sealed class InstanceMethod<P1, P2, P3, P4, P5, P6, P7> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
    where P5 : DyObject
    where P6 : DyObject
    where P7 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!, Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args), Cast<P6>(4, args), Cast<P7>(5, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1, P2, P3, P4, P5, P6, P7>(FunctionName, fun, Parameters);
}

internal sealed class InstanceMethod<P1, P2, P3, P4, P5, P6, P7, P8> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
    where P5 : DyObject
    where P6 : DyObject
    where P7 : DyObject
    where P8 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, P8?, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, P8?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!, Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args), Cast<P6>(4, args), Cast<P7>(5, args), Cast<P8>(6, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1, P2, P3, P4, P5, P6, P7, P8>(FunctionName, fun, Parameters);
}

internal sealed class InstanceMethod<P1, P2, P3, P4, P5, P6, P7, P8, P9> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
    where P5 : DyObject
    where P6 : DyObject
    where P7 : DyObject
    where P8 : DyObject
    where P9 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, P8?, P9?, DyObject> fun;

    public InstanceMethod(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, P8?, P9?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, (P1)Self!, Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args), Cast<P6>(4, args), Cast<P7>(5, args), Cast<P8>(6, args), Cast<P9>(7, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new InstanceMethod<P1, P2, P3, P4, P5, P6, P7, P8, P9>(FunctionName, fun, Parameters);
}