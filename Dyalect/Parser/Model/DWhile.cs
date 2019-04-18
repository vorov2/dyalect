using System.Text;

namespace Dyalect.Parser.Model
{
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
}
