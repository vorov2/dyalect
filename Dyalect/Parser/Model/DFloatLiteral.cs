using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DFloatLiteral : DNode
{
    public DFloatLiteral(Location loc) : base(NodeType.Float, loc) { }

    public double Value { get; set; }

    internal override void ToString(StringBuilder sb) =>
        sb.Append(Value.ToString(InvariantCulture.NumberFormat));
}
