using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DLabelLiteral : DNode
    {
        public DLabelLiteral(Location loc) : base(NodeType.Label, loc) { }

        public string Label { get; set; }

        public DNode Expression { get; set; }

        protected internal override string GetName() => Label;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append(Label);
            sb.Append(" = ");
            Expression.ToString(sb);
        }
    }
}
