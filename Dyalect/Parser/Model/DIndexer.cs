using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DIndexer : DNode
    {
        public DIndexer(Location loc) : base(NodeType.Index, loc) { }

        public DNode Target { get; set; }

        public DNode Index { get; set; }

        public bool NilSafety { get; set; }

        protected internal override string GetName() => Index.GetName();

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);

            if (NilSafety)
                sb.Append('?');

            sb.Append('[');
            Index.ToString(sb);
            sb.Append(']');
        }
    }
}
