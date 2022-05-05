using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public sealed class DyString : DyCollection
{
    public static readonly DyString Empty = new("");

    public override string TypeName => nameof(Dy.String);

    public readonly string Value;

    private int hashCode;

    public override int Count => Value.Length;

    public DyString(string str) : base(Dy.String) => Value = str;

    public DyString(HashString str) : base(Dy.String) => (Value, hashCode) = ((string)str, str.LookupHash());

    public static DyString Get(string? val) => string.IsNullOrEmpty(val) ? Empty : new(val);

    public override DyObject[] ToArray()
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

    protected internal override DyObject[] UnsafeAccess() => throw new NotImplementedException();
}
