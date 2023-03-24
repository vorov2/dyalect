using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DReturn : DNode
{
    public DReturn(Location loc) : base(NodeType.Return, loc) { }

    public DNode? Expression { get; set; }

    internal override void ToString(StringBuilder sb)
    {
        sb.Append("return");

        if (Expression is not null)
        {
            sb.Append(' ');
            Expression.ToString(sb);
        }
    }
}
