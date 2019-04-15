using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{


    public sealed class DFunctionDeclaration : DNode
    {
        public DFunctionDeclaration(Location loc) : base(NodeType.Function, loc)
        {

        }

        public bool IsMemberFunction => TypeName != null;

        public Qualident TypeName { get; set; }

        public string Name { get; set; }

        public bool Variadic { get; set; }

        public List<string> Parameters { get; } = new List<string>();

        public DNode Body { get; set; }

        internal override void ToString(StringBuilder sb)
        {
            sb.Append("func ");

            if (TypeName != null)
            {
                sb.Append(TypeName);
                sb.Append('.');
            }

            if (Name != null)
                sb.Append(Name);

            sb.Append('(');
            sb.Append(string.Join(",", Parameters));

            if (Variadic)
                sb.Append("...");

            sb.Append(") ");
            Body.ToString(sb);
        }
    }
}
