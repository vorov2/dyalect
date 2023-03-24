using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DRebinding : DBindingBase
{
    public DRebinding(Location loc) : base(NodeType.Rebinding, loc) { }

    internal override void ToString(StringBuilder sb)
    {
        sb.Append("set");
        Pattern.ToString(sb);
        sb.Append(" = ");
        Init.ToString(sb);
    }
}
