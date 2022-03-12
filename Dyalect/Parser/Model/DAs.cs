using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DAs : DNode
    {
        public DAs(Location loc) : base(NodeType.As, loc) { }

        public DNode Expression { get; set; } = null!;

        public Qualident TypeName { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            Expression?.ToString(sb);
            sb.Append(" as ");
            sb.Append(TypeName);
        }
    }
}
