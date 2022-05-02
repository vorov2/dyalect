using System.Collections.Generic;
using System.IO;
using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

public sealed class DyString : DyCollection
{
    public static readonly DyString Empty = new("");

    public override string TypeName => nameof(Dy.String);

    internal readonly string Value;

    private int hashCode;

    public override int Count => Value.Length;

    public DyString(string str) : base(Dy.String) => Value = str;

    public DyString(HashString str) : base(Dy.String) => (Value, hashCode) = ((string)str, str.LookupHash());

    public static DyString Get(string? val) => string.IsNullOrEmpty(val) ? Empty : new(val);
    
    internal override DyObject GetValue(int index) => new DyChar(Value[index]);

    internal override DyObject[] GetValues()
    {
        var arr = new DyObject[Value.Length];

        for (var i = 0; i < Value.Length; i++)
            arr[i] = new DyChar(Value[i]);

        return arr;
    }

    private IEnumerable<DyChar> Iterate()
    {
        for (var i = 0; i < Value.Length; i++)
            yield return new DyChar(Value[i]);
    }

    public override IEnumerator<DyObject> GetEnumerator() => Iterate().GetEnumerator();
    
    public override object ToObject() => Value;

    public override string ToString() => Value;

    public override int GetHashCode()
    {
        if (hashCode == 0)
            hashCode = Value.GetHashCode();

        return hashCode;
    }

    public override bool Equals(DyObject? obj) => obj is DyString s && Value == s.Value;

    public override DyObject Clone() => this;

    public static explicit operator string(DyString str) => str.Value;

    internal DyObject GetItem(DyObject index, ExecutionContext ctx)
    {
        if (index.TypeId != Dy.Integer)
            return ctx.IndexOutOfRange(index);

        return GetItem((int)index.GetInteger(), ctx);
    }

    protected override DyObject CollectionGetItem(int idx, ExecutionContext ctx) => new DyChar(Value[idx]);

    protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx) =>
        ctx.OperationNotSupported("set", Dy.String);
}
