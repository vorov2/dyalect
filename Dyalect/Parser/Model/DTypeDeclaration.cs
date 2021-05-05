using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTypeDeclaration : DNode
    {
        public DTypeDeclaration(Location loc) : base(NodeType.Type, loc) { }

        public string Name { get; set; } = null!;

        public bool HasConstructors => constructors is not null && constructors.Count > 0;

        private List<DFunctionDeclaration> constructors = null!;
        public List<DFunctionDeclaration> Constructors => constructors ??= new();

        public DNode? With { get; set; }

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

            if (With is not null)
            {
                sb.Append(" with ");
                With.ToString(sb);
            }
        }
    }
}
