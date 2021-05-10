using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public abstract class DNode
    {
        protected DNode(NodeType type, Location loc) => (NodeType, Location) = (type, loc);

        public NodeType NodeType { get; }

        public Location Location { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        protected internal virtual string? GetName() => null;

        protected internal virtual int GetElementCount() => -1;

        protected internal virtual List<DNode>? ListElements() => null;

        protected internal virtual bool HasAuto() => false;

        internal abstract void ToString(StringBuilder sb);
    }
}
