using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class SortComparer : IComparer<DyObject>
    {
        private readonly DyFunction? fun;
        private readonly ExecutionContext ctx;

        public SortComparer(DyFunction? fun, ExecutionContext ctx)
        {
            this.fun = fun;
            this.ctx = ctx;
        }

        public int Compare(DyObject? x, DyObject? y)
        {
            if (x is null || y is null)
                return 0;

            if (x.TypeCode == DyType.Label)
                x = x.GetTaggedValue();

            if (y.TypeCode == DyType.Label)
                y = y.GetTaggedValue();

            if (fun is not null)
            {
                var ret = fun.Call(ctx, x, y);

                if (ctx.HasErrors)
                    return 0;

                return ret.TypeCode != DyType.Integer
                    ? (ret.TypeCode != DyType.Float ? (int)ret.GetFloat() : 0)
                    : (int)ret.GetInteger();
            }

            var res = ctx.RuntimeContext.Types[x.TypeCode].Gt(ctx, x, y);

            if (ctx.HasErrors)
                return 0;
            
            if (ReferenceEquals(res, DyBool.True))
                return 1;

            res = ctx.RuntimeContext.Types[x.TypeCode].Eq(ctx, x, y);

            if (ctx.HasErrors)
                return 0;
            
            return ReferenceEquals(res, DyBool.True) ? 0 : -1;
        }
    }
}
