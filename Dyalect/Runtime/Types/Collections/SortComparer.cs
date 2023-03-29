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

        if (x is DyLabel la1)
            x = la1.Value;

        if (y is DyLabel la2)
            y = la2.Value;

        if (func is not null)
        {
            var ret = func.Call(ctx, x, y);
            ctx.ThrowIf();
            return ret is DyInteger i ? (int)i.Value
                : (ret is DyFloat f ? (int)f.Value : 0);
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
