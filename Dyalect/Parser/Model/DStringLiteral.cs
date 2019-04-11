using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DStringLiteral : DNode
    {
        public DStringLiteral(Location loc) : base(NodeType.String, loc)
        {

        }

        public string Value { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(StringUtil.Escape(Value));
        }
    }
}
