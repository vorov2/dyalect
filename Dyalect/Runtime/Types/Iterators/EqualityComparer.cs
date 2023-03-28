using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dyalect.Runtime.Types;

public sealed class EqualityComparer : IEqualityComparer<DyObject>
{
    private readonly ExecutionContext ctx;
    private readonly DyFunction func;

    public EqualityComparer(ExecutionContext ctx, DyObject functor)
    {
        this.ctx = ctx;
        func = functor.ToFunction(ctx)!;
        ctx.ThrowIf();
    }

    public bool Equals(DyObject? x, DyObject? y)
    {
        var fst = func.Call(ctx, x!);
        var snd = func.Call(ctx, y!);
        return fst.Equals(snd, ctx);
    }

    public int GetHashCode([DisallowNull] DyObject obj)
    {
        var x = func.Call(ctx, obj);
        return x.GetHashCode();
    }
}
