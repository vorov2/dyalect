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

            if (x.TypeId == DyType.Label)
                x = x.GetTaggedValue();

            if (y.TypeId == DyType.Label)
                y = y.GetTaggedValue();

            if (fun is not null)
            {
                var ret = fun.Call(ctx, x, y);

                if (ctx.HasErrors)
                    return 0;

                return ret.TypeId != DyType.Integer
                    ? (ret.TypeId == DyType.Float ? (int)ret.GetFloat() : 0)
                    : (int)ret.GetInteger();
            }

            var res = x.Greater(y, ctx);

            if (ctx.HasErrors)
                return 0;
            
            if (res)
                return 1;

            res = x.Equals(y, ctx);

            if (ctx.HasErrors)
                return 0;
            
            return res ? 0 : -1;
        }
    }
}
