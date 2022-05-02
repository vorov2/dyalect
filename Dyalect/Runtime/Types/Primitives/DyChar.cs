using System.IO;
namespace Dyalect.Runtime.Types;

public sealed class DyChar : DyObject
{
    public static readonly DyChar WhiteSpace = new(' ');
    public static readonly DyChar Empty = new('\0');
    public static readonly DyChar Max = new(char.MaxValue);
    public static readonly DyChar Min = new(char.MinValue);

    internal readonly char Value;

    public override string TypeName => nameof(Dy.Char); 

    public DyChar(char value) : base(Dy.Char) => this.Value = value;

    public override object ToObject() => Value;

    public override string ToString() => Value.ToString();

    public override DyObject Clone() => this;

    public override bool Equals(DyObject? other) => other is DyChar c && c.Value == Value;

    public override int GetHashCode() => Value.GetHashCode();
}
