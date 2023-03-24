using System.Text;

namespace Dyalect.Parser.Model;

public abstract class DNode
{
    public NodeType NodeType { get; }

    public Location Location { get; }

    protected DNode(NodeType type, Location loc) => (NodeType, Location) = (type, loc);

    public override string ToString()
    {
        var sb = new StringBuilder();
        ToString(sb);
        return sb.ToString();
    }

    internal abstract void ToString(StringBuilder sb);
}
