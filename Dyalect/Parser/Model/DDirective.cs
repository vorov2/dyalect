using System.Collections.Generic;
using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DDirective : DNode
{
    public DDirective(Location loc) : base(NodeType.Directive, loc) { }

    public string Key { get; set; } = null!;

    public List<object> Attributes { get; } = new();

    internal override void ToString(StringBuilder sb)
    {
        sb.Append('#');
        sb.Append(Key);

        if (Attributes.Count > 0)
        {
            sb.Append(" (");

            foreach (var o in Attributes)
            {
                sb.Append(o?.ToString());
                sb.Append(' ');
            }

            sb.Append(')');
        }
    }
}
