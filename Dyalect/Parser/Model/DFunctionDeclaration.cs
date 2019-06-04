using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DFunctionDeclaration : DNode
    {
        public DFunctionDeclaration(Location loc) : base(NodeType.Function, loc)
        {

        }

        public bool IsMemberFunction => TypeName != null;

        public Qualident TypeName { get; set; }

        public string Name { get; set; }

        public bool IsStatic { get; set; }

        public bool IsIterator { get; set; }

        public List<DParameter> Parameters { get; } = new List<DParameter>();

        public DNode Body { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            if (IsStatic)
                sb.Append("static ");

            if (Name != null)
                sb.Append("func ");

            if (TypeName != null)
            {
                sb.Append(TypeName);
                sb.Append('.');
            }

            if (Name != null)
                sb.Append(Name);

            if (Name != null || Parameters.Count > 1)
                sb.Append('(');

            Parameters.ToString(sb);

            if (Name != null || Parameters.Count > 1)
                sb.Append(") ");

            if (Name == null)
                sb.Append(" => ");

            Body.ToString(sb);
        }
    }
}
