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
            if (DyObject.Is(x, DyLabel.Type))
                x = x.GetTaggedValue();

            if (DyObject.Is(y, DyLabel.Type))
                y = y.GetTaggedValue();

            if (fun is not null)
            {
                var ret = fun.Call(ctx, x, y);

                if (ctx.HasErrors)
                    return 0;

                return !DyObject.Is(ret, DyInteger.Type)
                    ? (DyObject.Is(ret, DyFloat.Type) ? (int)ret.GetFloat() : 0)
                    : (int)ret.GetInteger();
            }

            var res = x.DecType.Gt(ctx, x, y);

            if (ctx.HasErrors)
                return 0;
            
            if (ReferenceEquals(res, DyBool.True))
                return 1;

            res = x.DecType.Eq(ctx, x, y);

            if (ctx.HasErrors)
                return 0;
            
            return ReferenceEquals(res, DyBool.True) ? 0 : -1;
        }
    }
}
