using Dyalect.Compiler;
using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

public static class Func
{
    public static DyFunction Compose(DyFunction first, DyFunction second) => new CompositionContainer(first, second);

    public static DyFunction Variant(string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(name, fun, pars, varArgIndex, FunAttr.Vari);
    
    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject> fun) => new MemberFunction0(name, fun);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction1(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction2(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction3(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction4(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction5(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction6(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction7(name, fun, pars, varArgIndex);

    public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction8(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject> fun) => new StaticFunction0(name, fun, Array.Empty<Par>());

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction2(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction3(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction4(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction5(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction6(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction7(name, fun, pars, varArgIndex);

    public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction8(name, fun, pars, varArgIndex);
    
    public static DyFunction Auto(string name, Func<ExecutionContext, DyObject> fun) => new AutoFunction0(name, fun);

    public static DyFunction Auto(string name, Func<ExecutionContext, DyObject, DyObject> fun) => new AutoFunction1(name, fun);

    public static DyFunction Method<P1, P2>(string name, Func<ExecutionContext, P1, P2, DyObject?> fun, Par par)
        where P1 : DyObject
        where P2 : DyObject
        => new TypedMemberFunction1<P1, P2>(name, fun!, par);
}


internal sealed class TypedMemberFunction1<P1, P2> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, DyObject> fun;

    public TypedMemberFunction1(string name, Func<ExecutionContext, P1?, P2?, DyObject> fun, params Par[] pars) : base(name, pars) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, Cast<P1>(), Cast<P2>(0, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new TypedMemberFunction1<P1, P2>(FunctionName, fun, Parameters[0]);
}

internal sealed class TypedMemberFunction2<P1, P2, P3> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, DyObject> fun;

    public TypedMemberFunction2(string name, Func<ExecutionContext, P1?, P2?, P3?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, Cast<P1>(), Cast<P2>(0, args), Cast<P3>(1, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new TypedMemberFunction2<P1, P2, P3>(FunctionName, fun, Parameters);
}

internal sealed class TypedMemberFunction3<P1, P2, P3, P4> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, DyObject> fun;

    public TypedMemberFunction3(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, Cast<P1>(), Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new TypedMemberFunction3<P1, P2, P3, P4>(FunctionName, fun, Parameters);
}

internal sealed class TypedMemberFunction4<P1, P2, P3, P4, P5> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
    where P5 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, DyObject> fun;

    public TypedMemberFunction4(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, Cast<P1>(), Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new TypedMemberFunction4<P1, P2, P3, P4, P5>(FunctionName, fun, Parameters);
}

internal sealed class TypedMemberFunction5<P1, P2, P3, P4, P5, P6> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
    where P5 : DyObject
    where P6 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, DyObject> fun;

    public TypedMemberFunction5(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, Cast<P1>(), Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args), Cast<P6>(4, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new TypedMemberFunction5<P1, P2, P3, P4, P5, P6>(FunctionName, fun, Parameters);
}

internal sealed class TypedMemberFunction6<P1, P2, P3, P4, P5, P6, P7> : DyForeignFunction
    where P1 : DyObject
    where P2 : DyObject
    where P3 : DyObject
    where P4 : DyObject
    where P5 : DyObject
    where P6 : DyObject
    where P7 : DyObject
{
    private readonly Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, DyObject> fun;

    public TypedMemberFunction6(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, Cast<P1>(), Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args), Cast<P6>(4, args), Cast<P7>(5, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new TypedMemberFunction6<P1, P2, P3, P4, P5, P6, P7>(FunctionName, fun, Parameters);
}

internal sealed class TypedMemberFunction7<P1, P2, P3, P4, P5, P6, P7, P8> : DyForeignFunction
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

    public TypedMemberFunction7(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, P8?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, Cast<P1>(), Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args), Cast<P6>(4, args), Cast<P7>(5, args), Cast<P8>(6, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new TypedMemberFunction7<P1, P2, P3, P4, P5, P6, P7, P8>(FunctionName, fun, Parameters);
}

internal sealed class TypedMemberFunction8<P1, P2, P3, P4, P5, P6, P7, P8, P9> : DyForeignFunction
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

    public TypedMemberFunction8(string name, Func<ExecutionContext, P1?, P2?, P3?, P4?, P5?, P6?, P7?, P8?, P9?, DyObject> fun, params Par[] pars) : base(name, pars, -1) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var ret = fun(ctx, Cast<P1>(), Cast<P2>(0, args), Cast<P3>(1, args), Cast<P4>(2, args), Cast<P5>(3, args), Cast<P6>(4, args), Cast<P7>(5, args), Cast<P8>(6, args), Cast<P9>(7, args));
        return ret ?? DyNil.Instance;
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new TypedMemberFunction8<P1, P2, P3, P4, P5, P6, P7, P8, P9>(FunctionName, fun, Parameters);
}
