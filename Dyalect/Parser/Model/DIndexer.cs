using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DIndexer : DNode
    {
        public DIndexer(Location loc) : base(NodeType.Index, loc)
        {

        }

        public DNode Target { get; set; }

        public DNode Index { get; set; }

        public string FieldName { get; set; }

        protected internal override string GetName() => FieldName ?? Index.GetName();

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);

            if (Index.NodeType == NodeType.Name)
            {
                sb.Append('.');
                Index.ToString(sb);
            }
            else
            {
                sb.Append('[');
                Index.ToString(sb);
                sb.Append(']');
            }
        }
    }
}
