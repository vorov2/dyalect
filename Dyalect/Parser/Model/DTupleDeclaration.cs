using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTupleDeclaration : DNode
    {
        public DTupleDeclaration(Location loc) : base(NodeType.Tuple, loc)
        {

        }

        public List<DNode> Elements { get; } = new List<DNode>();

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('(');
            Elements.ToString(sb);
            sb.Append(')');
        }
    }
}
