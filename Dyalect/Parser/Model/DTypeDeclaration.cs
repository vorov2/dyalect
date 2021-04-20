using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTypeDeclaration : DNode
    {
        public DTypeDeclaration(Location loc) : base(NodeType.Type, loc) { }

        public string Name { get; set; }

        public bool HasConstructors => constructors is not null && constructors.Count > 0;

        private List<DFunctionDeclaration> constructors;
        public List<DFunctionDeclaration> Constructors => constructors ??= new();

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("type ");
            sb.Append(Name);

            if (HasConstructors)
            {
                sb.Append(" = ");
                var fst = true;

                foreach (var c in constructors)
                {
                    if (!fst)
                        sb.Append(" | ");

                    sb.Append(c.Name);
                    sb.Append('(');
                    c.Parameters.ToString(sb);
                    sb.Append(')');
                }
            }
        }
    }
}
