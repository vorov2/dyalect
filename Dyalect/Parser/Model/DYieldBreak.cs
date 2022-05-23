using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DYieldBreak : DNode
{
    public DYieldBreak(Location loc) : base(NodeType.YieldBreak, loc) { }

    internal override void ToString(StringBuilder sb) => sb.Append("yield break");
}
