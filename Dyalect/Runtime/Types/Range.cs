using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal static class Range
    {
        public static IEnumerable<DyObject> GenerateRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step)
        {
            if (to.TypeId != DyType.Nil && from.TypeId != to.TypeId)
            {
                ctx.InvalidType(to);
                yield break;
            }

            var elem = from;

            if (step.TypeId == DyType.Nil)
                step = DyInteger.One;

            var inf = to.TypeId == DyType.Nil;

            if (inf)
            {
                while (true)
                {
                    yield return elem;
                    elem = ctx.RuntimeContext.Types[elem.TypeId].Add(ctx, elem, step);

                    if (ctx.HasErrors)
                        yield break;
                }
            }
            else
            {
                var up = ctx.RuntimeContext.Types[step.TypeId].Gt(ctx, step, DyInteger.Zero).GetBool();

                if (ctx.HasErrors)
                    yield break;

                var types = ctx.RuntimeContext.Types[from.TypeId];
                while ((up ? types.Lte(ctx, elem, to) : types.Gte(ctx, elem, to)).GetBool())
                {
                    yield return elem;
                    elem = ctx.RuntimeContext.Types[elem.TypeId].Add(ctx, elem, step);

                    if (ctx.HasErrors)
                        yield break;
                }
            }
        }
    }
}
