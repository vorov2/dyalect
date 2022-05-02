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

    public DyTuple(DyObject[] values) : this(values, values.Length) { }

    internal DyTuple(DyObject[] values, bool mutable, bool vararg) : this(values, values.Length) =>
        (this.mutable, IsVarArg) = (mutable, vararg);

    public DyTuple(DyObject[] values, int length) : base(Dy.Tuple)
    {
        this.length = length;
        this.values = values ?? throw new DyException("Unable to create a tuple with no values.");
    }

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

    public static DyTuple Create(params DyLabel[] labels) => new(labels, labels.Length);

    public Dictionary<DyObject, DyObject> ConvertToDictionary(ExecutionContext _)
    {
        var dict = new Dictionary<DyObject, DyObject>();

        for (var i = 0; i < Count; i++)
        {
            var ki = GetKeyInfo(i);
            var v = GetValue(i);
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

        item = CollectionGetItem(i, null!);
        return true;
    }

    protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
    {
        if (index.TypeId == Dy.Integer)
            return GetItem((int)index.GetInteger(), ctx);

        if (index.TypeId != Dy.String && index.TypeId != Dy.Char)
            return ctx.IndexOutOfRange(index);

        return TryGetItem(index.GetString(),out var item)
            ? item : ctx.IndexOutOfRange(index);
    }

    protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
    {
        if (index.TypeId == Dy.String)
        {
            var i = GetOrdinal(index.GetString());

            if (i is -1)
            {
                ctx.IndexOutOfRange(index);
                return;
            }

            CollectionSetItem(i, value, ctx);
        }
        else
            base.SetItem(index, value, ctx);
    }

    public virtual int GetOrdinal(string name)
    {
        for (var i = 0; i < Count; i++)
            if (values[i].GetLabel() == name)
                return i;

        return -1;
    }

    public virtual bool IsReadOnly(int index) => values[index] is DyLabel lab && !lab.Mutable;

    protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) =>
        values[index].TypeId == Dy.Label ? values[index].GetTaggedValue() : values[index];

    internal virtual string? GetKey(int index) => values[index].GetLabel();

    protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx)
    {
        if (values[index].TypeId == Dy.Label)
        {
            var lab = (DyLabel)values[index];

            if (!lab.Mutable)
            {
                ctx.IndexReadOnly(lab.Label);
                return;
            }

            if (!lab.VerifyType(value.TypeId))
            {
                ctx.InvalidType(value);
                return;
            }

            lab.Value = value;
        }
        else
            ctx.IndexReadOnly();
    }

    private static string DefaultKey() => Guid.NewGuid().ToString();

    internal override DyObject GetValue(int index) => values[index].GetTaggedValue();

    internal virtual void SetValue(int index, DyObject value)
    {
        if (values[index] is DyLabel lab)
            lab.Value = value;
        else
            values[index] = value;
    }

    internal virtual DyLabel? GetKeyInfo(int index) => values[index] is DyLabel lab ? lab : null;

    internal override DyObject[] GetValues()
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
            arr[i] = values[i].GetTaggedValue();

        return arr;
    }

    internal override bool IsMutable()
    {
        if (mutable is not null)
            return mutable.Value;

        for (var i = 0; i < Count; i++)
            if (values[i].IsMutable())
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

    internal DyObject[] UnsafeAccessValues() => values;
}
