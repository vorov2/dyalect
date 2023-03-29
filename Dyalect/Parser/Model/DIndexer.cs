using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DIndexer : DNode
{
    public DIndexer(Location loc) : base(NodeType.Index, loc) { }

    public DNode Target { get; set; } = null!;

    public DNode Index { get; set; } = null!;

    internal override void ToString(StringBuilder sb)
    {
        Target.ToString(sb);
        sb.Append('[');
        Index.ToString(sb);
        sb.Append(']');
    }
}
