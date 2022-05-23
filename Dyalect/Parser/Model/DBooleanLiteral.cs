using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DBooleanLiteral : DNode
{
    public DBooleanLiteral(Location loc) : base(NodeType.Boolean, loc) { }

    public bool Value { get; set; }

    internal override void ToString(StringBuilder sb) =>
        sb.Append(Value ? "true" : "false");
}
