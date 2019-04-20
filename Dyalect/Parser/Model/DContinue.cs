using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DContinue : DNode
    {
        public DContinue(Location loc) : base(NodeType.Continue, loc)
        {

        }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("continue");
        }
    }
}
