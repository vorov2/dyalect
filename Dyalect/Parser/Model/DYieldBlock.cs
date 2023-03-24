using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DYieldBlock : DNode
{
    public DYieldBlock(Location loc) : base(NodeType.YieldBlock, loc) { }

    public List<DNode> Elements { get; } = new();

    internal override void ToString(StringBuilder sb) => Elements.ToString(sb);
}
