using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DRegion : DNode
{
    public string? GlobalError { get; set; }

    public string? FileName { get; set; }

    public string Name { get; }

    public DyCodeModel Body { get; }

    public DRegion(string name, DyCodeModel body, Location loc) : base(NodeType.TestBlock, loc) =>
        (Name, Body) = (name, body);

    internal override void ToString(StringBuilder sb)
    {
        sb.AppendLine($"#region {Name}");
        sb.Append(Body.ToString());
        sb.AppendLine("#endregion");
    }
}
