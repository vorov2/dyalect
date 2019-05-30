using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DBinding : DNode
    {
        public DBinding(Location loc) : base(NodeType.Binding, loc)
        {

        }

        public bool Rebinding { get; set; }

        public bool Constant { get; set; }

        public DPattern Pattern { get; internal set; }

        public DNode Init { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            if (Rebinding)
                sb.Append("set");
            else
                sb.Append(Constant ? "const " : "var ");

            Pattern.ToString(sb);

            if (Init != null)
            {
                sb.Append(" = ");
                Init.ToString(sb);
            }
        }
    }
}
