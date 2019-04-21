using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DIntegerLiteral : DNode
    {
        public DIntegerLiteral(Location loc) : base(NodeType.Integer, loc)
        {

        }

        public long Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Value.ToString(CI.NumberFormat));
        }
    }

    public sealed class DFloatLiteral : DNode
    {
        public DFloatLiteral(Location loc) : base(NodeType.Float, loc)
        {

        }

        public double Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Value.ToString(CI.NumberFormat));
        }
    }

    public sealed class DBooleanLiteral : DNode
    {
        public DBooleanLiteral(Location loc) : base(NodeType.Boolean, loc)
        {

        }

        public bool Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Value ? "true" : "false");
        }
    }

    public sealed class DBinaryOperation : DNode
    {
        public DBinaryOperation(Location loc) : base(NodeType.Binary, loc)
        {

        }

        public DBinaryOperation(DNode left, DNode right, BinaryOperator op, Location loc) : this(loc)
        {
            Left = left;
            Right = right;
            Operator = op;
        }

        public DNode Left { get; set; }

        public DNode Right { get; set; }

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

    public sealed class DUnaryOperation : DNode
    {
        public DUnaryOperation(Location loc) : base(NodeType.Unary, loc)
        {

        }

        public DUnaryOperation(DNode node, UnaryOperator op, Location loc) : this(loc)
        {
            Node = node;
            Operator = op;
        }

        public DNode Node { get; set; }

        public UnaryOperator Operator { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Operator.ToSymbol());
            Node.ToString(sb);
        }
    }
}
