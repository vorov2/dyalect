using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DYieldMany : DNode
    {
        public DYieldMany(Location loc) : base(NodeType.YieldMany, loc) { }

        public DNode Expression { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("yield many ");
            Expression.ToString(sb);
        }
    }
}
