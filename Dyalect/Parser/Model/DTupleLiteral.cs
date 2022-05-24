using System.Collections.Generic;
using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DTupleLiteral : DNode, INodeContainer
{
    public DTupleLiteral(Location loc) : base(NodeType.Tuple, loc) { }

    public List<DNode> Elements { get; } = new();

    public int NodeCount => Elements.Count;

    internal override void ToString(StringBuilder sb)
    {
        sb.Append('(');
        Elements.ToString(sb);
        sb.Append(')');
    }
}
