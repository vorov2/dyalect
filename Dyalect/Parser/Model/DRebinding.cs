using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DRebinding : DNode
    {
        public DRebinding(Location loc) : base(NodeType.Rebinding, loc)
        {

        }

        public DPattern Pattern { get; internal set; }

        public DNode Init { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("set");
            Pattern.ToString(sb);
            sb.Append(" = ");
            Init.ToString(sb);
        }
    }
}
