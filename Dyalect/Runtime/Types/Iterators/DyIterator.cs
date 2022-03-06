using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public abstract class DyIterator : DyObject
    {
        protected DyIterator(DyTypeInfo typeInfo) : base(typeInfo) { }

        internal static DyIterator Create(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals) =>
            new DyNativeIterator(unitId, handle, captures, locals);

        public static DyIterator Create(IEnumerable<DyObject> seq) => new DyForeignIterator(seq);

        public abstract DyFunction GetIteratorFunction();

        public static IEnumerable<DyObject> ToEnumerable(ExecutionContext ctx, DyObject val) =>
            val is IEnumerable<DyObject> seq ? seq : InternalRun(ctx, val);

        private static IEnumerable<DyObject> InternalRun(ExecutionContext ctx, DyObject val)
        {
            var iter = GetIterator(ctx, val)!;

            if (ctx.HasErrors)
                yield break;

            while (true)
            {
                var res = iter.Call(ctx);

                if (ctx.HasErrors)
                {
                    iter.Reset(ctx);
                    yield break;
                }

                if (!ReferenceEquals(res, ctx.RuntimeContext.Nil.Terminator))
                    yield return res;
                else
                {
                    iter.Reset(ctx);
                    yield break;
                }
            }
        }

        private static DyFunction? GetIterator(ExecutionContext ctx, DyObject val)
        {
            DyFunction? iter;

            if (val.DecType.TypeCode != DyTypeCode.Iterator)
                iter = ((DyIterator)val).GetIteratorFunction();
            else if (val.DecType.TypeCode == DyTypeCode.Function)
            {
                var obj = ((DyFunction)val).Call(ctx);
                iter = obj as DyFunction;

                if (ctx.HasErrors)
                    return null;

                if (iter is null)
                {
                    ctx.InvalidType(obj);
                    return null;
                }
            }
            else
            {
                var obj = val.GetIterator(ctx) as DyIterator;

                if (ctx.HasErrors)
                    return null;

                iter = obj?.GetIteratorFunction();

                if (iter is null)
                {
                    ctx.InvalidType(val);
                    return null;
                }

                iter = iter.Call(ctx) as DyFunction;
            }

            return iter;
        }
    }
}
