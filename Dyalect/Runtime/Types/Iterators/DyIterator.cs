using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Runtime.Types
{
    public abstract class DyIterator : DyObject
    {
        protected DyIterator() : base(DyType.Iterator) { }

        internal static DyIterator Create(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals) =>
            new DyNativeIterator(unitId, handle, captures, locals);

        public static DyIterator Create(IEnumerable<DyObject> seq) => new DyForeignIterator(seq);

        public abstract DyFunction GetIteratorFunction();

        public static IEnumerable<DyObject> ToEnumerable(ExecutionContext ctx, DyObject val)
        {
            if (val.TypeId == DyType.Array)
                return ((DyArray)val).Values;

            if (val.TypeId == DyType.Tuple)
                return ((DyTuple)val).Values;

            if (val.TypeId == DyType.String)
                return (DyString)val;

            return InternalRun(ctx, val);
        }

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

                if (!ReferenceEquals(res, DyNil.Terminator))
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

            if (val.TypeId == DyType.Iterator)
                iter = ((DyIterator)val).GetIteratorFunction();
            else if (val.TypeId == DyType.Function)
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
