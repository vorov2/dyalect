using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTupleLiteral : DNode
    {
        public DTupleLiteral(Location loc) : base(NodeType.Tuple, loc) { }

        public List<DNode> Elements { get; } = new();

        protected internal override int GetElementCount() => Elements.Count;

        protected internal override List<DNode> ListElements() => Elements;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('(');
            Elements.ToString(sb);
            sb.Append(')');
        }
    }
}
