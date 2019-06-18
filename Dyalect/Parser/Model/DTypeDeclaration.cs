using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTypeDeclaration : DNode
    {
        public DTypeDeclaration(Location loc) : base(NodeType.Type, loc)
        {

        }

        public string Name { get; set; }

        public bool HasConstructors => _constructors != null;

        private List<DFunctionDeclaration> _constructors;
        public List<DFunctionDeclaration> Constructors
        {
            get
            {
                if (_constructors == null)
                    _constructors = new List<DFunctionDeclaration>();
                return _constructors;
            }
        }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("type ");
            sb.Append(Name);

            if (HasConstructors)
            {
                sb.Append(" = ");
                var fst = true;

                foreach (var c in _constructors)
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
