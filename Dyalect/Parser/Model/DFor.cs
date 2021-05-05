using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DFor : DNode
    {
        public DFor(Location loc) : base(NodeType.For, loc) { }

        public DPattern Pattern { get; set; } = null!;

        public DNode Target { get; set; } = null!;

        public DNode? Guard { get; set; }

        public DNode Body { get; set; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("for ");
            Pattern.ToString(sb);
            sb.Append(" in ");
            Target.ToString(sb);

            if (Guard is not null)
            {
                sb.Append(" when ");
                Guard.ToString(sb);
            }

            Body.ToString(sb);
        }
    }
}
