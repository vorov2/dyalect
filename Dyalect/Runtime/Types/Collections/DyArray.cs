﻿using System.Collections.Generic;

namespace Dyalect.Runtime.Types;

public class DyArray : DyCollection, IEnumerable<DyObject>
{
    private const int DefaultSize = 4;

    private DyObject[] values;

    public override string TypeName => nameof(Dy.Array);
    
    public DyObject this[int index]
    {
        get => values[index];
        set => values[index] = value;
    }

    public DyArray(DyObject[] values) : base(Dy.Array) => 
        (this.values, Count) = (values, values.Length);

    public override bool Equals(DyObject? other) => ReferenceEquals(this, other);

    private int CorrectIndex(int index) => index < 0 ? values.Length + index : index;

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
        var xs = new List<DyObject>(values);
        xs.RemoveRange(start, count);
        values = xs.ToArray();
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

        if (index < 0 || index > Count)
            throw new DyCodeException(DyError.IndexOutOfRange, index);

        if (index == Count && values.Length > index)
        {
            values[index] = item;
            Count++;
            Version++;
            return;
        }

        values = EnsureSize(Count + 1, values);
        Array.Copy(values, index, values, index + 1, Count - index);
        values[index] = item;
        Count++;
        Version++;

        static DyObject[] EnsureSize(int size, DyObject[] values)
        {
            if (size > values.Length)
            {
                var exp = values.Length * 2;

                if (size > exp)
                    exp = size;

                var arr = new DyObject[exp];
                Array.Copy(values, arr, values.Length);
                return arr;
            }

            return values;
        }
    }

    public void RemoveAt(int index)
    {
        index = CorrectIndex(index);

        if (index < 0 || index >= Count)
            throw new IndexOutOfRangeException();
        
        Count--;
        Array.Copy(values, index + 1, values, index, Count - index);
        values[Count] = null!;
        Version++;
    }

    public void Clear()
    {
        Count = 0;
        values = new DyObject[DefaultSize];
        Version++;
    }

    internal int IndexOf(ExecutionContext ctx, DyObject value)
    {
        for (var i = 0; i < Count; i++)
        {
            var e = values[i];

            if (e.Equals(value, ctx))
                return i;
        }

        return -1;
    }
    
    public int LastIndexOf(ExecutionContext ctx, DyObject value)
    {
        var index = -1;

        for (var i = 0; i < values.Length; i++)
        {
            var e = values[i];

            if (e.Equals(value, ctx))
                index = i;

            if (ctx.HasErrors)
                return -1;
        }

        return index;
    }

    public override IEnumerator<DyObject> GetEnumerator() => new DyCollectionEnumerator(values, 0, Count, this);

    public override DyObject[] ToArray()
    {
        var arr = new DyObject[Count];

        for (var i = 0; i < Count; i++)
            arr[i] = values[i];

        return arr;
    }

    internal protected override DyObject[] UnsafeAccess() => values;
}
