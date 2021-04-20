
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DParameter : DNode
    {
        public DParameter(Location loc) : base(NodeType.Parameter, loc) { }

        public string Name { get; set; }

        public DNode DefaultValue { get; set; }

        public bool IsVarArgs { get; set; }

        protected internal override string GetName() => Name;

        internal override void ToString(StringBuilder sb)
        {
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
}
