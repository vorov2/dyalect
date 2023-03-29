using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DIteratorLiteral : DNode
{
    public DIteratorLiteral(Location loc) : base(NodeType.Iterator, loc) { }

    public DYieldBlock YieldBlock { get; set; } = null!;

    internal override void ToString(StringBuilder sb)
    {
        sb.Append("yields {");
        YieldBlock.ToString(sb);
        sb.Append('}');
    }
}
