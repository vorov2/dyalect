using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DBinding : DBindingBase
{
    public DBinding(Location loc) : base(NodeType.Binding, loc) { }

    public bool AutoClose { get; set; }

    public bool Constant { get; set; }

    public bool Lazy { get; set; }

    internal override void ToString(StringBuilder sb)
    {
        if (AutoClose)
            sb.Append("auto ");

        sb.Append(Constant ? "let " : Lazy ? "lazy" : "var ");
        Pattern.ToString(sb);

        if (Init is not null)
        {
            sb.Append(" = ");
            Init.ToString(sb);
        }
    }
}
