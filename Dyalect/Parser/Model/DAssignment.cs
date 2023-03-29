using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DAssignment : DNode
{
    public DAssignment(Location loc) : base(NodeType.Assignment, loc) { }

    public BinaryOperator? AutoAssign { get; set; }

    public DNode Target { get; set; } = null!;

    public DNode Value { get; set; } = null!;

    internal override void ToString(StringBuilder sb)
    {
        Target.ToString(sb);

        if (AutoAssign is not null)
        {
            sb.Append(' ');
            sb.Append(AutoAssign.Value.ToSymbol());
            sb.Append("= ");
            Value.ToString(sb);
        }
        else
        {
            sb.Append(" = ");
            Value.ToString(sb);
        }
    }
}
