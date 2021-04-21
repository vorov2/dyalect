using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DIteratorLiteral : DNode
    {
        public DIteratorLiteral(Location loc) : base(NodeType.Iterator, loc) { }

        public DYieldBlock YieldBlock { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('{');
            YieldBlock.ToString(sb);
            sb.Append('}');
        }
    }

    public sealed class DYieldBlock : DNode
    {
        public DYieldBlock(Location loc) : base(NodeType.YieldBlock, loc) { }

        public List<DNode> Elements { get; } = new();

        internal override void ToString(StringBuilder sb) => Elements.ToString(sb);
    }
}
