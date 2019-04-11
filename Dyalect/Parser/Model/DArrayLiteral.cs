using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DArrayLiteral : DNode
    {
        public DArrayLiteral(Location loc) : base(NodeType.Array, loc)
        {

        }

        public List<DNode> Elements { get; } = new List<DNode>();

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('[');
            Elements.ToString(sb);
            sb.Append(']');
        }
    }
}
