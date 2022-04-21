using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class SortComparer : IComparer<DyObject>
    {
        private readonly DyObject functor;
        private readonly ExecutionContext ctx;

        public SortComparer(DyObject functor, ExecutionContext ctx)
        {
            this.functor = functor;
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

            if (functor.NotNil())
            {
                var ret = functor.Invoke(ctx, x, y);
                ctx.ThrowIf();
                return ret.TypeId != DyType.Integer
                    ? (ret.TypeId == DyType.Float ? (int)ret.GetFloat() : 0)
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
}
