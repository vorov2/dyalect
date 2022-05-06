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

        public List<Qualident>? Mixins { get; internal set; }

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

                sb.Append(c.Name);
                sb.Append('(');
                c.Parameters.ToString(sb);
                sb.Append(')');
            }

            if (Mixins is not null)
            {
                sb.Append(" with ");
                for (var i = 0; i < Mixins.Count; i++)
                {
                    if (i > 0)
                        sb.Append(',');
                    
                    sb.Append(Mixins[i].ToString());
                }
            }
        }
    }
}
