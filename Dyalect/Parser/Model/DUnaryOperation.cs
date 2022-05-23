using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DUnaryOperation : DNode
{
    public DUnaryOperation(Location loc) : base(NodeType.Unary, loc) { }

    public DUnaryOperation(DNode node, UnaryOperator op, Location loc) : this(loc) =>
        (Node, Operator) = (node, op);

    public DNode Node { get; set; } = null!;

    public UnaryOperator Operator { get; set; }

    internal override void ToString(StringBuilder sb)
    {
        sb.Append(Operator.ToSymbol());
        Node.ToString(sb);
    }
}
