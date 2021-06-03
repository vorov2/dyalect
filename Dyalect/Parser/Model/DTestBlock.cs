using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DTestBlock : DNode
    {
        public DTestBlock(Location loc) : base(NodeType.TestBlock, loc) { }

        public string Name { get; init; } = null!;

        public DNode Body { get; init; } = null!;

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("test ");
            sb.Append(Name);
            Body.ToString(sb);
        }
    }
}
