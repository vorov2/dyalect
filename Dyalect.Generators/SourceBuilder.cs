using System.Text;

namespace Dyalect.Generators
{
    internal sealed class SourceBuilder
    {
        private readonly StringBuilder builder = new();
        private int padding;

        public void Indent() => padding += 4;
        public void Outdent() => padding -= 4;
        
        public void StartBlock()
        {
            AppendLine("{");
            Indent();
        }
        public void EndBlock()
        {
            Outdent();
            AppendLine("}");
        }

        public void AppendPadding() => builder.Append(new string(' ', padding));
        public void Append(string value) => builder.Append(value);
        public void AppendLine(string value = "") => builder.AppendLine(new string(' ', padding) + value);
        public void AppendInBlock(string value)
        {
            AppendLine("{");
            Indent();
            AppendLine(value);
            Outdent();
            AppendLine("}");
        }
        public override string ToString() => builder.ToString();
    }
}
