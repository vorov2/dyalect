using System.Collections.Generic;
using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DArrayLiteral : DNode, INodeContainer
{
    public DArrayLiteral(Location loc) : base(NodeType.Array, loc) { }

    public List<DNode> Elements { get; } = new();

    public int NodeCount => 
        Elements.Count == 1 && Elements[0].NodeType == NodeType.Range ? -1 : Elements.Count;

    internal override void ToString(StringBuilder sb)
    {
        sb.Append('[');
        Elements.ToString(sb);
        sb.Append(']');
    }
}
