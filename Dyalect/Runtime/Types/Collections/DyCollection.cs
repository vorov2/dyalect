using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public abstract class DyCollection : DyEnumerable
{
    protected DyCollection(int typeCode) : base(typeCode) { }

    public override object ToObject() => ToTypedArray();

    private Array ToTypedArray()
    {
        if (Count is 0)
            return Array.Empty<object>();

        var xs = ToArray();
        var fe = xs[0].ToObject();

        if (fe is not null && TypeConverter.TryCreateTypedArray(xs, fe.GetType(), out var result))
            return result!;

        var newArr = new object[Count];

        for (var i = 0; i < newArr.Length; i++)
            newArr[i] = xs[i].ToObject();

        return newArr;
    }

    public abstract DyObject[] ToArray();

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
