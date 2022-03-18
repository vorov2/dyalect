
using System.Text;

namespace Dyalect.Parser.Model
{
    public class DParameter : DNode
    {
        public DParameter(Location loc) : base(NodeType.Parameter, loc) { }

        public string Name { get; set; } = null!;

        public DNode? DefaultValue { get; set; }

        public TypeAnnotation? TypeAnnotation { get; set; }

        public bool IsVarArgs { get; set; }

        protected internal override string? GetName() => Name;

        internal override void ToString(StringBuilder sb)
        {
            if (TypeAnnotation is not null)
            {
                sb.Append(TypeAnnotation.ToString());
                sb.Append(' ');
            }

            sb.Append(Name);

            if (DefaultValue is not null)
            {
                sb.Append(" = ");
                DefaultValue.ToString(sb);
            }

            if (IsVarArgs)
                sb.Append("...");
        }
    }

    public sealed class DTypeParameter : DParameter
    {
        public DTypeParameter(Location loc) : base(loc) { }
        
        public bool Mutable { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            if (Mutable)
                sb.Append("var ");

            base.ToString(sb);
        }
    }
}
