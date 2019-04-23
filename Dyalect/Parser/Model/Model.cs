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
}
