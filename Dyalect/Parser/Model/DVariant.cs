using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DVariant : DNode
{
    public DVariant(string name, Location loc) : base(NodeType.Variant, loc) =>
        Name = name;

    public string Name { get; }

    public List<DNode> Arguments { get; } = new();

    internal override void ToString(StringBuilder sb)
    {
        sb.Append('@');
        sb.Append(Name);
        sb.Append('(');
        Arguments.ToString(sb);
        sb.Append(')');
    }
}
