using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DNilLiteral : DNode
    {
        public DNilLiteral(Location loc) : base(NodeType.Nil, loc)
        {

        }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("nil");
        }
    }
}
