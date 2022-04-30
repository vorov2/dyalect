using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

internal sealed class SortComparer : IComparer<DyObject>
{
    private readonly DyFunction? func;
    private readonly ExecutionContext ctx;

    public SortComparer(DyFunction? functor, ExecutionContext ctx)
    {
        this.func = functor;
        this.ctx = ctx;
    }

    public int Compare(DyObject? x, DyObject? y)
    {
        if (x is null || y is null)
            return 0;

        if (x.Is(Dy.Label))
            x = x.GetTaggedValue();

        if (y.Is(Dy.Label))
            y = y.GetTaggedValue();

        if (func is not null)
        {
            var ret = func.Call(ctx, x, y);
            ctx.ThrowIf();
            return !ret.Is(Dy.Integer)
                ? (ret.Is(Dy.Float) ? (int)ret.GetFloat() : 0)
                : (int)ret.GetInteger();
        }

        var res = x.Greater(y, ctx);
        ctx.ThrowIf();
        
        if (res)
            return 1;

        res = x.Equals(y, ctx);
        ctx.ThrowIf();
        return res ? 0 : -1;
    }
}
