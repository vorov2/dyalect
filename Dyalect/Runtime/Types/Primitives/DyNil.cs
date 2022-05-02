using System.IO;
namespace Dyalect.Runtime.Types;

public class DyNil : DyObject
{
    internal const string Literal = "nil";
    public static readonly DyNil Instance = new();
    internal static readonly DyNil Terminator = new DyNilTerminator();

    public override string TypeName => nameof(Dy.Nil); 

    private sealed class DyNilTerminator : DyNil { }

    private DyNil() : base(Dy.Nil) { }

    public override object ToObject() => null!;

    public override string ToString() => Literal;

    public override bool Equals(DyObject? other) => ReferenceEquals(this, other);

    public override DyObject Clone() => this;

    public override int GetHashCode() => HashCode.Combine(TypeName, TypeId);
}
