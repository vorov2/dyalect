using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public abstract class DNode
    {
        protected DNode(NodeType type, Location loc)
        {
            NodeType = type;
            Location = loc;
        }

        public NodeType NodeType { get; }

        public Location Location { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        internal protected virtual string GetName() => null;

        internal abstract void ToString(StringBuilder sb);
    }

    public sealed class DBlock : DNode
    {
        public DBlock(Location loc) : base(NodeType.Block, loc)
        {

        }

        public List<DNode> Nodes { get; } = new List<DNode>();

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("{ ");
            Nodes.ToString(sb, "");
            sb.Append(" } ");
        }
    }

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

    public sealed class DStringLiteral : DNode
    {
        public DStringLiteral(Location loc) : base(NodeType.String, loc)
        {

        }

        public string Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(StringUtil.Escape(Value));
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

    public sealed class DNilLiteral : DNode
    {
        public DNilLiteral(Location loc) : base(NodeType.Nil, loc)
        {

        }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("nil");
        }
    }

    public sealed class DName : DNode
    {
        public DName(Location loc) : base(NodeType.Name, loc)
        {

        }

        public string Value { get; set; }

        protected internal override string GetName() => Value;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Value);
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

    public sealed class DBinding : DNode
    {
        public DBinding(Location loc) : base(NodeType.Binding, loc)
        {

        }

        public bool Constant { get; set; }

        public string Name { get; internal set; }

        public DNode Init { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Constant ? "const " : "var ");
            sb.Append(Name);

            if (Init != null)
            {
                sb.Append(" = ");
                Init.ToString(sb);
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

    public sealed class DMixin : DNode
    {
        public DMixin(Location loc) : base(NodeType.Mixin, loc)
        {

        }

        public DNode Target { get; set; }

        public string Field { get; set; }

        protected internal override string GetName() => Field;

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);
            sb.Append('.');
            sb.Append(Field);
        }
    }

    public sealed class DIndexer : DNode
    {
        public DIndexer(Location loc) : base(NodeType.Index, loc)
        {

        }

        public DNode Target { get; set; }

        public DNode Index { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);
            sb.Append('[');
            Index.ToString(sb);
            sb.Append(']');
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

    public sealed class DFunctionDeclaration : DNode
    {
        public DFunctionDeclaration(Location loc) : base(NodeType.Function, loc)
        {

        }

        public bool IsMemberFunction => TypeName != null;

        public Qualident TypeName { get; set; }

        public string Name { get; set; }

        public bool Variadic { get; set; }

        public List<string> Parameters { get; } = new List<string>();

        public DNode Body { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("func ");

            if (TypeName != null)
            {
                sb.Append(TypeName);
                sb.Append('.');
            }

            if (Name != null)
                sb.Append(Name);

            sb.Append('(');
            sb.Append(string.Join(",", Parameters));

            if (Variadic)
                sb.Append("...");

            sb.Append(") ");
            Body.ToString(sb);
        }
    }

    public sealed class DImport : DNode
    {
        public DImport(Location loc) : base(NodeType.Import, loc)
        {

        }

        public string Alias { get; set; }

        public string ModuleName { get; set; }

        public string Dll { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("import ");

            if (Alias != null && Alias != ModuleName)
            {
                sb.Append(Alias);
                sb.Append('=');
            }

            sb.Append(ModuleName);

            if (Dll != null)
            {
                sb.Append('(');
                sb.Append(Dll);
                sb.Append(')');
            }
        }
    }
}
