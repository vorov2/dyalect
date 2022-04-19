using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dyalect.Runtime.Types
{
    public sealed class EqualityComparer : IEqualityComparer<DyObject>
    {
        private readonly ExecutionContext ctx;
        private readonly DyObject func;

        public EqualityComparer(ExecutionContext ctx, DyObject func) => 
            (this.ctx, this.func) = (ctx, func);

        public bool Equals(DyObject? x, DyObject? y)
        {
            var fst = func.Invoke(ctx, x!);
            if (ctx.HasErrors) throw new BreakException();
            var snd = func.Invoke(ctx, y!);
            if (ctx.HasErrors) throw new BreakException();
            return ctx.RuntimeContext.Types[fst.TypeId].Eq(ctx, fst, snd).IsTrue();
        }

        public int GetHashCode([DisallowNull] DyObject obj)
        {
            var x = func.Invoke(ctx, obj);
            return x.GetHashCode();
        }
    }
}
