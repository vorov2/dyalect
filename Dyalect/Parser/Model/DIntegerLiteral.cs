using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DIntegerLiteral : DNode
    {
        public DIntegerLiteral(Location loc) : base(NodeType.Integer, loc) { }

        public long Value { get; set; }

        internal override void ToString(StringBuilder sb) =>
            sb.Append(Value.ToString(CI.NumberFormat));
    }
}
