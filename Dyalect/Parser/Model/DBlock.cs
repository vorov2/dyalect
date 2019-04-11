using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DBlock : DNode
    {
        public DBlock(Location loc) : base(NodeType.Block, loc)
        {

        }

        public List<DNode> Nodes { get; } = new List<DNode>();

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("{ ");
            Nodes.ToString(sb, "");
            sb.Append(" } ");
        }
    }
}
