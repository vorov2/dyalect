using Dyalect.Codegen;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal abstract partial class DyCollTypeInfo : DyTypeInfo
{
    #region Operations
    protected override DyObject LengthOp(ExecutionContext ctx, DyObject self) =>
        DyInteger.Get(((DyEnumerable)self).Count);

    protected override DyObject IterateOp(ExecutionContext ctx, DyObject self)
    {
        if (self is IEnumerable<DyObject> seq)
            return DyIterator.Create(seq);

        return Nil;
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType)
    {
        if (targetType.ReflectedTypeId == self.TypeId)
            return self;
        
        var xs = (DyCollection)self;
        return targetType.ReflectedTypeId switch
        {
            Dy.Tuple => new DyTuple(xs.ToArray()),
            Dy.Array => new DyArray(xs.ToArray()),
            Dy.Iterator => DyIterator.Create(xs),
            Dy.Set => new DySet(new HashSet<DyObject>(xs.ToArray())),
            _ => base.CastOp(ctx, self, targetType)
        };
    }
    #endregion

    [InstanceMethod]
    internal static IEnumerable<DyObject> Indices(DyCollection self)
    {
        IEnumerable<DyObject> Iterate()
        {
            for (var i = 0; i < self.Count; i++)
                yield return DyInteger.Get(i);
        }

        return Iterate();
    }

    [InstanceMethod]
    internal static IEnumerable<DyObject> Slice(DyCollection self, int index, [Default]int? size)
    {
        var arr = self.UnsafeAccess();

        if (size is null)
            size = self.Count - 1;

        if (index == 0 && size == arr.Length - 1)
            return self;

        if (index < 0)
            index = self.Count + index;

        if (index >= self.Count)
            throw new DyCodeException(DyError.IndexOutOfRange);

        if (size < 0)
            size = self.Count + size - 1;

        if (size >= self.Count || size < 0)
            throw new DyCodeException(DyError.IndexOutOfRange);

        var len = size.Value - index + 1;

        if (len < 0)
            throw new DyCodeException(DyError.IndexOutOfRange);

        return new DyCollectionEnumerable(arr, index, len, self);
    }

    [InstanceMethod]
    internal static HashSet<DyObject> ToSet(DyCollection self) => new (self.ToArray());
}
