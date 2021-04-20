using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DMatch : DNode
    {
        public DMatch(Location loc) : base(NodeType.Match, loc) { }

        public DNode Expression { get; set; }

        public List<DMatchEntry> Entries { get; } = new();

        internal override void ToString(StringBuilder sb)
        {
            if (Expression is not null)
            {
                sb.Append("match ");
                Expression.ToString(sb);
                sb.Append(' ');
            }

            sb.Append('{');

            foreach (var e in Entries)
            {
                e.ToString(sb);
                sb.Append(',');
            }

            sb.Append('}');
        }
    }
}
