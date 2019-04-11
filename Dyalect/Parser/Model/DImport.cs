using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DImport : DNode
    {
        public DImport(Location loc) : base(NodeType.Import, loc)
        {

        }

        public string Alias { get; set; }

        public string ModuleName { get; set; }

        public string Dll { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("import ");

            if (Alias != null && Alias != ModuleName)
            {
                sb.Append(Alias);
                sb.Append('=');
            }

            sb.Append(ModuleName);

            if (Dll != null)
            {
                sb.Append('(');
                sb.Append(Dll);
                sb.Append(')');
            }
        }
    }
}
