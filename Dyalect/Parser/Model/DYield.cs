using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DYield : DNode
    {
        public DYield(Location loc) : base(NodeType.Yield, loc)
        {

        }

        public DNode Expression { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("yield ");
            Expression.ToString(sb);
        }
    }
}
