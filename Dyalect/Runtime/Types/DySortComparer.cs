using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DySortComparer : IComparer<DyObject>
    {
        private readonly DyFunction fun;
        private readonly ExecutionContext ctx;

        public DySortComparer(DyFunction fun, ExecutionContext ctx)
        {
            this.fun = fun;
            this.ctx = ctx;
        }

        public int Compare(DyObject x, DyObject y)
        {
            if (x.TypeId == DyType.Label)
                x = x.GetTaggedValue();

            if (y.TypeId == DyType.Label)
                y = y.GetTaggedValue();

            if (fun != null)
            {
                var ret = fun.Call2(x, y, ctx);
                return ret.TypeId != DyType.Integer
                    ? (ret.TypeId == DyType.Float ? (int)ret.GetFloat() : 0)
                    : (int)ret.GetInteger();
            }

            var res = ctx.RuntimeContext.Types[x.TypeId].Gt(ctx, x, y);
            return ReferenceEquals(res, DyBool.True)
                ? 1
                : ReferenceEquals(ctx.RuntimeContext.Types[x.TypeId].Eq(ctx, x, y), DyBool.True) ? 0 : -1;
        }
    }
}
