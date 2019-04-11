using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTrait : DNode
    {
        public DTrait(Location loc) : base(NodeType.Trait, loc)
        {

        }

        public DNode Target { get; set; }

        public string Name { get; set; }

        protected internal override string GetName() => Name;

        internal override void ToString(StringBuilder sb)
        {
            Target.ToString(sb);
            sb.Append('.');
            sb.Append(Name);
        }
    }
}
