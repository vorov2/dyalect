using System;
using System.Collections.Generic;
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

    public sealed class DAssignment : DNode
    {
        public DAssignment(Location loc) : base(NodeType.Assignment, loc)
        {

        }

        public BinaryOperator? AutoAssign { get; set; }

        public DNode Target { get; set; }

        public DNode Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);

            if (AutoAssign != null)
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

    public sealed class DIf : DNode
    {
        public DIf(Location loc) : base(NodeType.If, loc)
        {

        }

        public DNode Condition { get; set; }

        public DNode True { get; set; }

        public DNode False { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("if ");
            Condition.ToString(sb);
            True.ToString(sb);

            if (False != null)
            {
                sb.Append("else ");
                False.ToString(sb);
            }
        }
    }

    public sealed class DApplication : DNode
    {
        public DApplication(DNode target, Location loc) : base(NodeType.Application, loc)
        {
            Target = target;
        }

        public DNode Target { get; }

        public List<DNode> Arguments { get; } = new List<DNode>();

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);
            sb.Append('(');
            Arguments.ToString(sb);
            sb.Append(')');
        }
    }

    public sealed class DWhile : DNode
    {
        public DWhile(Location loc) : base(NodeType.While, loc)
        {

        }

        public DNode Condition { get; set; }

        public DNode Body { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("while ");
            Condition.ToString(sb);
            Body.ToString(sb);
        }
    }

    public sealed class DBreak : DNode
    {
        public DBreak(Location loc) : base(NodeType.Break, loc)
        {

        }

        public DNode Expression { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("break");

            if (Expression != null)
            {
                sb.Append(' ');
                Expression.ToString(sb);
            }
        }
    }

    public sealed class DReturn : DNode
    {
        public DReturn(Location loc) : base(NodeType.Return, loc)
        {

        }

        public DNode Expression { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("return");

            if (Expression != null)
            {
                sb.Append(' ');
                Expression.ToString(sb);
            }
        }
    }

    public sealed class DContinue : DNode
    {
        public DContinue(Location loc) : base(NodeType.Continue, loc)
        {

        }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("continue");
        }
    }
}
