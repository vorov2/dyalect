using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DLabelLiteral : DNode
    {
        public DLabelLiteral(Location loc) : base(NodeType.Label, loc) { }
        
        public bool Mutable { get; init; }

        public string Label { get; init; } = null!;

        public bool FromString{ get; init; }

        public DNode Expression { get; init; } = null!;

        protected internal override string? GetName() => Label;

        internal override void ToString(StringBuilder sb)
        {
            if (Mutable)
                sb.Append("var ");

            if (FromString)
                sb.Append(StringUtil.Escape(Label));
            else
                sb.Append(Label);

            sb.Append(": ");
            Expression?.ToString(sb);
        }
    }
}
