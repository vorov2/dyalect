using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DName : DNode, INamedNode
{
    public DName(Location loc) : base(NodeType.Name, loc) { }

    public string Value { get; set; } = null!;

    public string NodeName => Value;

    internal override void ToString(StringBuilder sb) => sb.Append(Value);
}
