using System.Collections.Generic;
using System.IO;
using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

public sealed class DyString : DyCollection
{
    public static readonly DyString Empty = new("");

    internal readonly string Value;

    private int hashCode;

    public override int Count => Value.Length;

    public DyString(string str) : base(DyType.String) => Value = str;

    public DyString(HashString str) : base(DyType.String) => (Value, hashCode) = ((string)str, str.LookupHash());
    
    internal override DyObject GetValue(int index) => new DyChar(Value[index]);

    internal override DyObject[] GetValues()
    {
        var arr = new DyObject[Value.Length];

        for (var i = 0; i < Value.Length; i++)
            arr[i] = new DyChar(Value[i]);

        return arr;
    }

    internal override IEnumerable<DyObject> GetValuesIterator()
    {
        for (var i = 0; i < Value.Length; i++)
            yield return new DyChar(Value[i]);
    }

    public override object ToObject() => Value;

    public override string ToString() => Value;

    public override int GetHashCode()
    {
        if (hashCode == 0)
            hashCode = Value.GetHashCode();

        return hashCode;
    }

    public override bool Equals(DyObject? obj) =>
        obj is DyString s ? Value == s.Value : base.Equals(obj);

    public override DyObject Clone() => this;

    protected internal override string GetString() => Value;

    public static explicit operator string(DyString str) => str.Value;

    public static string ToString(DyObject value, ExecutionContext ctx)
    {
        var res = value;

        while (res.TypeId != DyType.String && res.TypeId != DyType.Char)
        {
            res = res.ToString(ctx);

            if (ctx.HasErrors)
                return null!;
        }

        return res.GetString();
    }

    protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
    {
        if (index.TypeId != DyType.Integer)
            return ctx.IndexOutOfRange(index);

        return GetItem((int)index.GetInteger(), ctx);
    }

    protected override DyObject CollectionGetItem(int idx, ExecutionContext ctx) => new DyChar(Value[idx]);

    protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx) =>
        ctx.OperationNotSupported("set", DyType.String);

    internal override void Serialize(BinaryWriter writer)
    {
        writer.Write(TypeId);
        writer.Write(Value);
    }
}
