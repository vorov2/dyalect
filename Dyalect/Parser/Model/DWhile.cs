using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DWhile : DNode
    {
        public DWhile(Location loc) : base(NodeType.While, loc) { }

        public DNode Condition { get; set; } = null!;

        public DNode Body { get; set; } = null!;

        public bool DoWhile { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            if (DoWhile)
            {
                sb.Append("do ");
                Body.ToString(sb);
                sb.Append(" while ");
                Condition.ToString(sb);
            }
            else
            {
                sb.Append("while ");
                Condition.ToString(sb);
                Body.ToString(sb);
            }
        }
    }
}
