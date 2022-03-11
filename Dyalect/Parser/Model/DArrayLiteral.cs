using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DArrayLiteral : DNode
    {
        public DArrayLiteral(Location loc) : base(NodeType.Array, loc) { }

        public List<DNode> Elements { get; } = new();

        protected internal override int GetElementCount() => 
            Elements.Count == 1 && Elements[0].NodeType == NodeType.Range ? -1 : Elements.Count;

        protected internal override List<DNode> ListElements() => Elements;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('[');
            Elements.ToString(sb);
            sb.Append(']');
        }
    }
}
