using Dyalect.Compiler;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public class DyTuple : DyCollection
{
    public static readonly DyTuple Empty = new(Array.Empty<DyObject>());

    public override string TypeName => nameof(Dy.Tuple);

    private readonly int length;
    private bool? mutable;
    private readonly DyObject[] values;

    public override int Count => length;

    public bool IsVarArg { get; }

    public DyObject this[int index]
    {
        get => values[index] is DyLabel la ? la.Value : values[index];
        set
        {
            if (values[index] is DyLabel la)
                la.Value = value;

            values[index] = value;
        }
    }

    public DyTuple(DyObject[] values) : this(values, values.Length) { }

    internal DyTuple(DyObject[] values, bool mutable, bool vararg) : this(values, values.Length) =>
        (this.mutable, IsVarArg) = (mutable, vararg);

    public DyTuple(DyObject[] values, int length) : base(Dy.Tuple)
    {
        this.length = length;
        this.values = values ?? throw new DyException("Unable to create a tuple with no values.");
    }

    public static DyTuple Create(params DyLabel[] values) => new(values, values.Length);

    public override IEnumerator<DyObject> GetEnumerator() => new DyCollectionEnumerator(values, 0, Count, this);

    public override DyObject Clone()
    {
        if (IsMutable())
            return base.Clone();

        return this;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;

            for (var i = 0; i < length; i++)
            {
                var v = values[i];
                hash = hash * 31 + v.GetHashCode();
            }

            return hash;
        }
    }

    public override bool Equals(DyObject? other)
    {
        if (other is null || other is not DyTuple xs)
            return false;

        if (xs.Count != length)
            return false;

        for (var i = 0; i < length; i++)
            if (!values[i].Equals(xs.values[i]))
                return false;

        return true;
    }

    public Dictionary<DyObject, DyObject> ConvertToDictionary()
    {
        var dict = new Dictionary<DyObject, DyObject>();

        for (var i = 0; i < Count; i++)
        {
            var ki = GetKeyInfo(i);
            var v = this[i];
            var key = new DyString(ki is null ? DefaultKey() : ki.Label);
            dict[key] = v;
        }

        return dict;
    }

    internal bool TryGetItem(string name, out DyObject item)
    {
        item = null!;
        var i = GetOrdinal(name);

        if (i is -1)
            return false;

        item = this[i];

        if (item is DyLabel lab)
            item = lab.Value;

        return true;
    }

    internal DyObject GetItem(ExecutionContext ctx, DyObject index)
    {
        if (index is DyInteger ix)
            return GetItem(ctx, (int)ix.Value);

        if (index.TypeId is Dy.String or Dy.Char && TryGetItem(index.ToString(), out var item))
            return item;

        return ctx.IndexOutOfRange(index);
    }

    internal DyObject GetItem(ExecutionContext ctx, int index)
    {
        index = index < 0 ? Count + index : index;

        if (index < 0 || index >= Count)
            return ctx.IndexOutOfRange(index);

        var item = values[index];

        if (item is DyLabel lab)
            item = lab.Value;

        return item;
    }

    internal void SetItem(ExecutionContext ctx, DyObject index, DyObject value)
    {
        int ix = -1;

        if (index.TypeId is Dy.String or Dy.Char)
            ix = GetOrdinal(index.ToString());
        else if (index is DyInteger i)
            ix = (int)(i.Value < 0 ? Count + i.Value : i.Value);

        if (ix < 0 || ix >= Count)
        {
            ctx.IndexOutOfRange(index);
            return;
        }

        if (values[ix] is DyLabel lab && lab.Mutable)
        {
            if (!lab.VerifyType(value.TypeId))
            {
                ctx.InvalidType(value);
                return;
            }

            lab.Value = value;
        }
        else
            ctx.IndexReadOnly(index);
    }

    public virtual int GetOrdinal(string name)
    {
        for (var i = 0; i < Count; i++)
            if (values[i] is DyLabel la && la.Label == name)
                return i;

        return -1;
    }

    public virtual bool IsReadOnly(int index) => values[index] is DyLabel lab && !lab.Mutable;

    internal virtual string? GetKey(int index) => values[index] is DyLabel la ? la.Label : null;

    private static string DefaultKey() => Guid.NewGuid().ToString();

    internal virtual void SetValue(int index, DyObject value)
    {
        if (values[index] is DyLabel lab)
            lab.Value = value;
        else
            values[index] = value;
    }

    internal virtual DyLabel? GetKeyInfo(int index) => values[index] is DyLabel lab ? lab : null;

    public override DyObject[] ToArray()
    {
        if (Count != values.Length)
            return CopyTuple();

        for (var i = 0; i < Count; i++)
            if (values[i].TypeId == Dy.Label)
                return CopyTuple();

        return values;
    }

    internal DyObject[] GetValuesWithLabels()
    {
        if (mutable != null)
        {
            if (!mutable.Value && Count == values.Length)
                return values;
            else
                return CopyTupleWithLabels();
        }

        if (Count != values.Length)
            return CopyTupleWithLabels();

        if (IsMutable())
            return CopyTupleWithLabels();

        return values;
    }

    private DyObject[] CopyTuple()
    {
        var arr = new DyObject[Count];

        for (var i = 0; i < Count; i++)
            arr[i] = values[i] is DyLabel la ? la.Value : values[i];

        return arr;
    }

    private bool IsMutable()
    {
        if (mutable is not null)
            return mutable.Value;

        for (var i = 0; i < Count; i++)
            if (values[i] is DyLabel la && la.Mutable)
            {
                mutable = true;
                return true;
            }

        mutable = false;
        return false;
    }

    private DyObject[] CopyTupleWithLabels()
    {
        var arr = new DyObject[Count];

        for (var i = 0; i < Count; i++)
            arr[i] = values[i] is DyLabel la ? new DyLabel(la.Label, la.Value) : values[i];

        return arr;
    }

    internal bool HasItem(string name) => GetOrdinal(name) is not -1;

    internal protected override DyObject[] UnsafeAccess() => values;

    private static DyObject Compare(bool gt, DyTuple xs, DyTuple ys, ExecutionContext ctx)
    {
        var xsv = xs.UnsafeAccess();
        var ysv = ys.UnsafeAccess();
        var len = xs.Count > ys.Count ? ys.Count : xs.Count;

        for (var i = 0; i < len; i++)
        {
            var x = xsv[i] is DyLabel lx ? lx.Value : xsv[i];
            var y = ysv[i] is DyLabel ly ? ly.Value : ysv[i];
            var res = gt ? x.Greater(y, ctx) : x.Lesser(y, ctx);

            if (res)
                return True;

            res = x.Equals(y, ctx);

            if (!res)
                return False;
        }

        return False;
    }

    internal static DyObject Greater(ExecutionContext ctx, DyTuple xs, DyTuple ys) => Compare(true, xs, ys, ctx);

    internal static DyObject Lesser(ExecutionContext ctx, DyTuple xs, DyTuple ys) => Compare(false, xs, ys, ctx);

    internal static DyObject Equals(ExecutionContext ctx, DyTuple xs, DyTuple ys)
    {
        if (xs.Count != ys.Count)
            return False;

        var t1v = xs.UnsafeAccess();
        var t2v = ys.UnsafeAccess();

        for (var i = 0; i < xs.Count; i++)
        {
            var x = t1v[i] is DyLabel lx ? lx.Value : t1v[i];
            var y = t2v[i] is DyLabel ly ? ly.Value : t2v[i];

            if (x.NotEquals(y, ctx))
                return False;
        }

        return True;
    }
}
