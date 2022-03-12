using Dyalect.Debug;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal abstract class DyCollectionTypeInfo : DyTypeInfo
    {
        protected DyCollectionTypeInfo() { }

        protected virtual DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
        {
            var coll = (DyCollection)self;
            var arr = coll.GetValues();

            if (fromElem.TypeId != DyType.Integer)
                return ctx.InvalidType(fromElem);

            if (toElem.TypeId != DyType.Nil && toElem.TypeId != DyType.Integer)
                return ctx.InvalidType(toElem);

            var beg = (int)fromElem.GetInteger();
            var end = ReferenceEquals(toElem, DyNil.Instance) ? coll.Count - 1 : (int)toElem.GetInteger();

            if (beg == 0 && end == coll.Count - 1)
                return self;

            if (beg < 0)
                beg = coll.Count + beg;

            if (beg >= coll.Count)
                return ctx.IndexOutOfRange();

            if (end < 0)
                end = coll.Count + end - 1;

            if (end >= coll.Count || end < 0)
                return ctx.IndexOutOfRange();

            var len = end - beg + 1;

            if (len < 0)
                return ctx.IndexOutOfRange();

            return DyIterator.Create(new DyCollectionEnumerable(arr, beg, len, coll));
        }

        protected DyObject GetIndices(ExecutionContext ctx, DyObject self)
        {
            var arr = (DyCollection)self;

            IEnumerable<DyObject> Iterate()
            {
                for (var i = 0; i < arr.Count; i++)
                    yield return DyInteger.Get(i);
            }

            return DyIterator.Create(Iterate());
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Indices" => Func.Member(name, GetIndices),
                "Slice" => Func.Member(name, GetSlice, -1, new Par("start", DyInteger.Zero), new Par("len", DyNil.Instance)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Tuple => new DyTuple(((DyCollection)self).ConvertToArray()),
                DyType.Array => new DyArray(((DyCollection)self).ConvertToArray()),
                DyType.Iterator => DyIterator.Create((DyCollection)self),
                _ => base.CastOp(self, targetType, ctx)
            };
    }
}
