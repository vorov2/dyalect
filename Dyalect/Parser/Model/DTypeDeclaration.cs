using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTypeDeclaration : DNode
    {
        internal static readonly DTypeDeclaration Default = new(default);

        public DTypeDeclaration(Location loc) : base(NodeType.Type, loc) { }

        public string Name { get; set; } = null!;

        public List<DFunctionDeclaration> Constructors { get; } = new();

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("type ");
            sb.Append(Name);
            sb.Append(" = ");
            var fst = true;

            foreach (var c in Constructors)
            {
                if (!fst)
                    sb.Append(" or ");

                if (c.IsPrivate)
                    sb.Append("private ");

                sb.Append(c.Name);
                sb.Append('(');
                c.Parameters.ToString(sb);
                sb.Append(')');
            }
        }
    }
}
