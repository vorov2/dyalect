using Dyalect.Codegen;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal abstract partial class DyCollectionTypeInfo : DyTypeInfo
{
    #region Operations
    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Tuple => new DyTuple(((DyCollection)self).Trim()),
            Dy.Array => new DyArray(((DyCollection)self).Trim()),
            Dy.Iterator => DyIterator.Create((DyCollection)self),
            Dy.Set => new DySet(new HashSet<DyObject>(((DyCollection)self).Trim())),
            _ => base.CastOp(self, targetType, ctx)
        };
    #endregion

    [InstanceMethod(Method.Indices)]
    internal static DyObject GetIndices(DyCollection self)
    {
        IEnumerable<DyObject> Iterate()
        {
            for (var i = 0; i < self.Count; i++)
                yield return DyInteger.Get(i);
        }

        return DyIterator.Create(Iterate());
    }

    [InstanceMethod(Method.Slice)]
    internal static DyObject GetSlice(ExecutionContext ctx, DyCollection self, int index, [Default]int? size)
    {
        var arr = self.GetValues();

        if (size is null)
            size = self.Count - 1;

        if (index == 0 && size == arr.Length - 1)
            return self;

        if (index < 0)
            index = self.Count + index;

        if (index >= self.Count)
            return ctx.IndexOutOfRange();

        if (size < 0)
            size = self.Count + size - 1;

        if (size >= self.Count || size < 0)
            return ctx.IndexOutOfRange();

        var len = size.Value - index + 1;

        if (len < 0)
            return ctx.IndexOutOfRange();

        return DyIterator.Create(new DyCollectionEnumerable(arr, index, len, self));
    }

    [InstanceMethod(Method.ToSet)]
    internal static DyObject ToSet(ExecutionContext ctx, DyCollection self)
    {
        var vals = self.GetValuesIterator();
        var set = new HashSet<DyObject>();
        set.UnionWith(vals);
        return new DySet(set);
    }
}
