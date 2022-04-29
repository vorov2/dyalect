using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DStringLiteral : DNode
    {
        public DStringLiteral(Location loc) : base(NodeType.String, loc) { }

        public string? Value { get; set; }

        public List<StringChunk>? Chunks { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            if (Chunks is not null)
            {
                var newSb = new StringBuilder();

                foreach (var c in Chunks)
                {
                    if (c is CodeChunk)
                    {
                        newSb.Append("\\(");
                        newSb.Append(c.GetContent());
                        newSb.Append(')');
                    }
                    else
                        newSb.Append(c.GetContent());
                }

                sb.Append(StringUtil.Escape(newSb.ToString()));
            }
            else if (Value is not null)
                sb.Append(StringUtil.Escape(Value));
        }
    }

    public abstract class StringChunk
    {
        public abstract string GetContent();

        public abstract bool IsCode { get; }
    }

    public sealed class PlainStringChunk : StringChunk
    {
        private readonly string value;

        internal PlainStringChunk(string value) => this.value = value;

        public override bool IsCode => false;

        public override string GetContent() => value;
    }

    public sealed class CodeChunk : StringChunk
    {
        private readonly string code;

        internal CodeChunk(string code) => this.code = code;

        public override bool IsCode => true;

        public override string GetContent() => code;
    }
}
