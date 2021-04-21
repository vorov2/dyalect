using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DCharLiteral : DNode
    {
        public DCharLiteral(Location loc) : base(NodeType.Char, loc) { }

        public char Value { get; set; }

        internal override void ToString(StringBuilder sb) =>
            sb.Append(StringUtil.Escape(Value.ToString(), "'"));
    }
}
