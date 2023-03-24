using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DBreak : DNode
{
    public DBreak(Location loc) : base(NodeType.Break, loc) { }

    public DNode? Expression { get; set; }

    internal override void ToString(StringBuilder sb)
    {
        sb.Append("break");

        if (Expression is not null)
        {
            sb.Append(' ');
            Expression.ToString(sb);
        }
    }
}
