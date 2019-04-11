using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DBinding : DNode
    {
        public DBinding(Location loc) : base(NodeType.Binding, loc)
        {

        }

        public bool Constant { get; set; }

        public string Name { get; internal set; }

        public DNode Init { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Constant ? "const " : "var ");
            sb.Append(Name);

            if (Init != null)
            {
                sb.Append(" = ");
                Init.ToString(sb);
            }
        }
    }
}
