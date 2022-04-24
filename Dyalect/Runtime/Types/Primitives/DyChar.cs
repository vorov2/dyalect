using System;
using System.Collections.Generic;
using System.IO;
namespace Dyalect.Runtime.Types;

public sealed class DyChar : DyStringLike
{
    public static readonly DyChar WhiteSpace = new(' ');
    public static readonly DyChar Empty = new('\0');
    public static readonly DyChar Max = new(char.MaxValue);
    public static readonly DyChar Min = new(char.MinValue);

    private readonly char value;

    public override string TypeName => DyTypeNames.Char; 

    public DyChar(char value) : base(DyType.Char) => this.value = value;

    public override object ToObject() => GetChar();

    protected internal override char GetChar() => value;

    protected internal override string GetString() => value.ToString();

    public override string ToString() => GetString();

    public override DyObject Clone() => this;

    internal override void Serialize(BinaryWriter writer)
    {
        writer.Write(TypeId);
        writer.Write(value);
    }

    public override bool Equals(DyObject? other) => other is DyChar c && c.value == value;

    public override int GetHashCode() => value.GetHashCode();


    #region Collection
    protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) => this;

    protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx) { }

    internal override DyObject GetValue(int index) => this;

    internal override DyObject[] GetValues() => Array.Empty<DyObject>();

    internal override IEnumerable<DyObject> GetValuesIterator()
    {
        yield return this;
    }
    #endregion
}
