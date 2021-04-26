using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    public sealed class DFunctionDeclaration : DNode
    {
        public DFunctionDeclaration(Location loc) : base(NodeType.Function, loc) { }

        public bool IsMemberFunction => TypeName != null;

        public Qualident TypeName { get; set; }

        public string Name { get; set; }

        internal bool IsStatic => Attribute is FunctionAttribute.Static or FunctionAttribute.Constructor;

        public FunctionAttribute Attribute { get; set; }

        public bool IsIterator { get; set; }

        public List<DParameter> Parameters { get; } = new();

        public DNode Body { get; set; }

        public bool IsVariadic()
        {
            for (var i = 0; i < Parameters.Count; i++)
            {
                if (Parameters[i].IsVarArgs)
                    return true;
            }

            return false;
        }

        internal override void ToString(StringBuilder sb)
        {
            if (Body is null)
            {
                sb.Append(Name);
                sb.Append('(');
                Parameters.ToString(sb);
                sb.Append(')');
                return;
            }

            if (Name is not null)
                sb.Append("func ");

            if (TypeName is not null)
            {
                sb.Append(TypeName);
                sb.Append('.');
            }

            if (Name is not null)
                sb.Append(Name);

            if (Name is not null || Parameters.Count > 1)
                sb.Append('(');

            Parameters.ToString(sb);

            if (Name is not null || Parameters.Count > 1)
                sb.Append(") ");

            if (Attribute == FunctionAttribute.Constructor)
                sb.Append("cons ");
            else if (Attribute == FunctionAttribute.Static)
                sb.Append("static ");
            else if (Attribute == FunctionAttribute.Deconstructor)
                sb.Append("decons ");

            if (Name is null)
                sb.Append(" => ");

            Body.ToString(sb);
        }
    }

    public enum FunctionAttribute
    {
        None,
        Static,
        Constructor,
        Deconstructor
    }
}
