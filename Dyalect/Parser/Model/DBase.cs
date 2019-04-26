using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DBase : DNode
    {
        public DBase(Location loc) : base(NodeType.Base, loc)
        {

        }

        public string Name { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("base.");
            sb.Append(Name);
        }
    }
}
