using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DIf : DNode
    {
        public DIf(Location loc) : base(NodeType.If, loc) { }

        public DNode Condition { get; set; } = null!;

        public DNode True { get; set; } = null!;

        public DNode? False { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("if ");
            Condition.ToString(sb);
            sb.Append(" { ");
            True.ToString(sb);
            sb.Append(" }");

            if (False is not null)
            {
                sb.Append("else { ");
                False.ToString(sb);
                sb.Append(" }");
            }
        }
    }
}
