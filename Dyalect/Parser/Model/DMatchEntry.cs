using System.Collections.Generic;
using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DMatchEntry : DNode
{
    public DMatchEntry(Location loc) : base(NodeType.MatchEntry, loc) { }

    public DPattern Pattern { get; set; } = null!;

    public DNode? Guard { get; set; }

    public DNode Expression { get; set; } = null!;

    internal override void ToString(StringBuilder sb)
    {
        Pattern?.ToString(sb);

        if (Guard is not null)
        {
            sb.Append(" when ");
            Guard.ToString(sb);
        }

        sb.Append(" => ");
        Expression?.ToString(sb);
    }
}
