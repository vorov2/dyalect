using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DName : DNode
    {
        public DName(Location loc) : base(NodeType.Name, loc) { }

        public string Value { get; set; }

        protected internal override string GetName() => Value;

        internal override void ToString(StringBuilder sb) => sb.Append(Value);
    }
}
