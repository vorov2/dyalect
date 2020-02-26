using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DPreprocessor : DNode
    {
        public DPreprocessor(Location loc) : base(NodeType.Preprocessor, loc)
        {

        }

        public string Key { get; set; }

        public List<object> Attributes { get; } = new List<object>();

        internal override void ToString(StringBuilder sb)
        {
            sb.Append('#');
            sb.Append(Key);

            if (Attributes.Count > 0)
            {
                sb.Append(" (");

                foreach (var o in Attributes)
                {
                    sb.Append(o?.ToString());
                    sb.Append(' ');
                }

                sb.Append(')');
            }
        }
    }
}
