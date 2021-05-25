using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DUsing : DNode
    {
        public DUsing(Location loc) : base(NodeType.Using, loc) { }

        public DNode Expression { get; init; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("using ");
            Expression.ToString(sb);
        }
    }
}
