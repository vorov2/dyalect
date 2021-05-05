using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DBinaryOperation : DNode
    {
        public DBinaryOperation(Location loc) : base(NodeType.Binary, loc) { }

        public DBinaryOperation(DNode left, DNode right, BinaryOperator op, Location loc) : this(loc) =>
            (Left, Right, Operator) = (left, right, op);

        public DNode Left { get; set; } = null!;

        public DNode Right { get; set; } = null!;

        public BinaryOperator Operator { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            Left.ToString(sb);
            sb.Append(' ');
            sb.Append(Operator.ToSymbol());
            sb.Append(' ');
            Right.ToString(sb);
        }
    }
}
