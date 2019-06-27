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

        public bool IsPrivate { get; set; }

        public bool IsConstructor { get; set; }

        public bool IsAuto { get; set; }

        public bool IsStatic { get; set; }

        public bool IsIterator { get; set; }

        public List<DParameter> Parameters { get; } = new List<DParameter>();

        public DNode Body { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            if (Body == null)
            {
                sb.Append(Name);
                sb.Append('(');
                Parameters.ToString(sb);
                sb.Append(')');
                return;
            }

            if (IsConstructor)
                sb.Append("cons ");
            else
            {
                if (IsStatic)
                    sb.Append("static ");

                if (IsAuto)
                    sb.Append("auto ");

                if (Name != null)
                    sb.Append("func ");
            }

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
