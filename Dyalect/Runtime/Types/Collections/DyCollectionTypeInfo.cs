using Dyalect.Debug;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

internal abstract class DyCollectionTypeInfo : DyTypeInfo
{
    protected DyCollectionTypeInfo() { }

    protected virtual DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
    {
        var coll = (DyCollection)self;
        var arr = coll.GetValues();

        if (!fromElem.IsInteger(ctx)) return Nil;
        if (toElem.NotNil() && !toElem.IsInteger(ctx)) return Nil;

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

    private DyObject ToSet(ExecutionContext ctx, DyObject self)
    {
        var vals = ((DyCollection)self).GetValuesIterator();
        var set = new HashSet<DyObject>();
        set.UnionWith(vals);
        return new DySet(set);
    }

    protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
        name switch
        {
            Method.Indices => Func.Member(name, GetIndices),
            Method.Slice => Func.Member(name, GetSlice, -1, new Par("index", DyInteger.Zero), new Par("size", DyNil.Instance)),
            Method.ToSet => Func.Member(name, ToSet),
            _ => base.InitializeInstanceMember(self, name, ctx)
        };

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Tuple => new DyTuple(((DyCollection)self).Trim()),
            DyType.Array => new DyArray(((DyCollection)self).Trim()),
            DyType.Iterator => DyIterator.Create((DyCollection)self),
            DyType.Set => new DySet(new HashSet<DyObject>(((DyCollection)self).Trim())),
            _ => base.CastOp(self, targetType, ctx)
        };
}
