using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DThrow : DNode
{
    public DThrow(Location loc) : base(NodeType.Throw, loc) { }

    public DNode? Expression { get; set; }

    internal override void ToString(StringBuilder sb)
    {
        sb.Append("throw ");
        Expression?.ToString(sb);
    }
}
