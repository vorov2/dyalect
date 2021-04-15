using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DRange : DNode
    {
        public DRange(Location loc) : base(NodeType.Range, loc) { }

        public bool Exclusive { get; set; }

        public DNode From { get; set; }

        public DNode To { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            From?.ToString(sb);
            sb.Append("..");
            
            if (Exclusive)
                sb.Append('<');

            To?.ToString(sb);
        }
    }
}
