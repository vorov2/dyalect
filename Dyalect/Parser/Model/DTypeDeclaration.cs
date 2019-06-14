using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTypeDeclaration : DNode
    {
        public DTypeDeclaration(Location loc) : base(NodeType.Type, loc)
        {

        }

        public string Name { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("type ");
            sb.Append(Name);
        }
    }
}
