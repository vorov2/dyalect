using Dyalect.Runtime.Types;
using System.Text;
namespace Dyalect.Library.Core;

public sealed class DyStringBuilder : DyForeignObject
{
    internal StringBuilder Builder;

    public DyStringBuilder(DyForeignTypeInfo typeInfo, StringBuilder builder) : base(typeInfo) => Builder = builder;

    public override bool Equals(DyObject? other) =>
        other is DyString || other is DyStringBuilder && Builder.ToString() == other.ToString();

    public override object ToObject() => Builder.ToString();

    public override string ToString() => Builder.ToString();

    public override int GetHashCode() => Builder.GetHashCode();

    public override DyObject Clone()
    {
        var clone = (DyStringBuilder)MemberwiseClone();
        clone.Builder = new StringBuilder(Builder.ToString());
        return clone;
    }
}
