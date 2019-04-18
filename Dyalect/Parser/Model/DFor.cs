using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DFor : DNode
    {
        public DFor(Location loc) : base(NodeType.For, loc)
        {

        }
        public DName Variable { get; set; }

        public DNode Target { get; set; }

        public DNode Body { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("for ");
            Variable.ToString(sb);
            sb.Append(" in ");
            Target.ToString(sb);
            Body.ToString(sb);
        }
    }
}
