using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DPrivateScope : DNode
    {
        public DPrivateScope(Location loc) : base(NodeType.PrivateScope, loc) { }

        public DBlock Block { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("private ");
            Block.ToString(sb);
        }
    }
}
