using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTupleLiteral : DNode
    {
        public DTupleLiteral(Location loc) : base(NodeType.Tuple, loc) { }

        public List<DNode> Elements { get; } = new();

        public List<int>? MutableFields { get; set; }

        internal protected override int GetElementCount() => Elements.Count;

        internal protected override List<DNode> ListElements() => Elements;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('(');
            Elements.ToString(sb);
            sb.Append(')');
        }
    }
}
