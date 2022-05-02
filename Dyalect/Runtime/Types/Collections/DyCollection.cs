using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public abstract class DyCollection : DyEnumerable
{
    protected DyCollection(int typeCode) : base(typeCode) { }

    #region Indexing
    internal int CorrectIndex(int index) => index < 0 ? Count + index : index;

    internal bool CorrectIndex(ExecutionContext ctx, ref int index)
    {
        var newIndex = CorrectIndex(index);

        if (newIndex < 0 || newIndex > Count - 1)
        {
            ctx.IndexOutOfRange(index);
            return false;
        }

        index = newIndex;
        return true;
    }

    internal DyObject GetItem(int index, ExecutionContext ctx)
    {
        index = CorrectIndex(index);
        
        if (index >= Count)
            return ctx.IndexOutOfRange(index);
        
        return CollectionGetItem(index, ctx);
    }

    protected abstract DyObject CollectionGetItem(int index, ExecutionContext ctx);

    protected internal override void SetItem(DyObject obj, DyObject value, ExecutionContext ctx)
    {
        if (obj.TypeId is not Dy.Integer)
        {
            ctx.InvalidType(Dy.Integer, obj);
            return;
        }

        var index = CorrectIndex((int)obj.GetInteger());

        if (index >= Count)
            ctx.IndexOutOfRange(index);
        else
            CollectionSetItem(index, value, ctx);
    }

    protected abstract void CollectionSetItem(int index, DyObject value, ExecutionContext ctx);
    #endregion

    public override object ToObject() => ConvertToArray();

    public Array ConvertToArray()
    {
        if (Count is 0)
            return Array.Empty<object>();

        var xs = GetValues();
        var fe = xs[0].ToObject();

        if (fe is not null && TypeConverter.TryCreateTypedArray(xs, fe.GetType(), out var result))
            return result!;

        var newArr = new object[Count];

        for (var i = 0; i < newArr.Length; i++)
            newArr[i] = xs[i].ToObject();

        return newArr;
    }

    internal abstract DyObject GetValue(int index);

    internal abstract DyObject[] GetValues();

    internal static DyObject[] ConcatValues(ExecutionContext ctx, params DyObject[] values)
    {
        if (values is null)
            return Array.Empty<DyObject>();

        var arr = new List<DyObject>();

        for (var i = 0; i < values.Length; i++)
        {
            var seq = DyIterator.ToEnumerable(ctx, values[i]);

            if (ctx.HasErrors)
                break;

            arr.AddRange(seq);
        }

        return arr.ToArray();
    }
}
