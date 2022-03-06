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

            if (x.DecType.TypeCode == DyTypeCode.Label)
                x = x.GetTaggedValue();

            if (y.DecType.TypeCode == DyTypeCode.Label)
                y = y.GetTaggedValue();

            if (fun is not null)
            {
                var ret = fun.Call(ctx, x, y);

                if (ctx.HasErrors)
                    return 0;

                return ret.DecType.TypeCode != DyTypeCode.Integer
                    ? (ret.DecType.TypeCode != DyTypeCode.Float ? (int)ret.GetFloat() : 0)
                    : (int)ret.GetInteger();
            }

            var res = x.DecType.Gt(ctx, x, y);

            if (ctx.HasErrors)
                return 0;
            
            if (ReferenceEquals(res, ctx.RuntimeContext.Bool.True))
                return 1;

            res = x.DecType.Eq(ctx, x, y);

            if (ctx.HasErrors)
                return 0;
            
            return ReferenceEquals(res, ctx.RuntimeContext.Bool.True) ? 0 : -1;
        }
    }
}
