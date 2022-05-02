using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public class DyArray : DyCollection, IEnumerable<DyObject>
{
    private const int DefaultSize = 4;

    private DyObject[] values;

    public override string TypeName => nameof(Dy.Array);
    
    public DyObject this[int index]
    {
        get => values[CorrectIndex(index)];
        set => values[CorrectIndex(index)] = value;
    }

    public DyArray(DyObject[] values) : base(Dy.Array) => 
        (this.values, Count) = (values, values.Length);

    public override bool Equals(DyObject? other) => ReferenceEquals(this, other);

    public void Compact()
    {
        if (Count == values.Length)
            return;

        var arr = new DyObject[Count];
        Array.Copy(values, arr, Count);
        values = arr;
        Version++;
    }

    public void RemoveRange(int start, int count)
    {
        var lst = new List<DyObject>(values);
        lst.RemoveRange(start, count);
        values = lst.ToArray();
        Count = values.Length;
        Version++;
    }

    public void Add(DyObject val)
    {
        if (Count == values.Length)
        {
            var dest = new DyObject[values.Length == 0 ? DefaultSize : values.Length * 2];
            Array.Copy(values, 0, dest, 0, Count);
            values = dest;
        }

        values[Count++] = val;
        Version++;
    }

    public void Insert(int index, DyObject item)
    {
        index = CorrectIndex(index);

        if (index > Count)
            throw new IndexOutOfRangeException();

        if (index == Count && values.Length > index)
        {
            values[index] = item;
            Count++;
            Version++;
            return;
        }

        EnsureSize(Count + 1);
        Array.Copy(values, index, values, index + 1, Count - index);
        values[index] = item;
        Count++;
        Version++;

        void EnsureSize(int size)
        {
            if (size > values.Length)
            {
                var exp = values.Length * 2;

                if (size > exp)
                    exp = size;

                var arr = new DyObject[exp];
                Array.Copy(values, arr, values.Length);
                values = arr;
            }
        }
    }

    public bool RemoveAt(int index)
    {
        index = CorrectIndex(index);

        if (index >= 0 && index < Count)
        {
            Count--;

            if (index < Count)
                Array.Copy(values, index + 1, values, index, Count - index);

            values[Count] = null!;
            Version++;
            return true;
        }

        return false;
    }

    public bool Remove(ExecutionContext ctx, DyObject val)
    {
        var index = IndexOf(ctx, val);

        if (index < 0)
            return false;

        return RemoveAt(index);
    }

    public void Clear()
    {
        Count = 0;
        values = new DyObject[DefaultSize];
        Version++;
    }

    internal int IndexOf(ExecutionContext ctx, DyObject elem)
    {
        for (var i = 0; i < Count; i++)
        {
            var e = values[i];

            if (e.Equals(elem, ctx))
                return i;

            if (ctx.HasErrors)
                return -1;
        }

        return -1;
    }
    
    public int LastIndexOf(ExecutionContext ctx, DyObject elem)
    {
        var index = -1;

        for (var i = 0; i < values.Length; i++)
        {
            var e = values[i];

            if (e.Equals(elem, ctx))
                index = i;

            if (ctx.HasErrors)
                return -1;
        }

        return index;
    }

    internal DyObject GetItem(DyObject index, ExecutionContext ctx)
    {
        if (index.Is(Dy.Integer))
            return GetItem((int)index.GetInteger(), ctx);
        else
            return ctx.IndexOutOfRange(index);
    }

    protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) => values[index];

    protected override void CollectionSetItem(int index, DyObject obj, ExecutionContext ctx) =>
        values[index] = obj;

    public override IEnumerator<DyObject> GetEnumerator() => new DyCollectionEnumerator(values, 0, Count, this);

    internal override DyObject GetValue(int index) => values[CorrectIndex(index)];

    internal override DyObject[] GetValues()
    {
        var arr = new DyObject[Count];

        for (var i = 0; i < Count; i++)
            arr[i] = values[i];

        return arr;
    }

    internal DyObject[] UnsafeAccessValues() => values;
}
