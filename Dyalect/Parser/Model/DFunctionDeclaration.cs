using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model;

public sealed class DFunctionDeclaration : DNode
{
    public DFunctionDeclaration(Location loc) : base(NodeType.Function, loc) { }

    public Qualident? TypeName { get; set; }

    public Qualident? TargetTypeName { get; set; }

    public string? Name { get; set; }

    internal bool IsStatic { get; set; }

    internal bool IsFinal { get; set; }

    internal bool IsAbstract => Body is null;

    internal bool IsIndexer { get; set; }

    internal bool IsConstructor { get; set; }

    internal bool IsNullary { get; set; }

    public bool Getter { get; set; }

    public bool Setter { get; set; }

    public bool IsIterator { get; set; }

    public List<DParameter> Parameters { get; } = new();

    public DNode? Body { get; set; }

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

        if (IsStatic)
            sb.Append("static ");

        if (IsFinal)
            sb.Append("final ");

        if (IsAbstract)
            sb.Append("abstract ");

        if (Name is not null)
            sb.Append("func ");

        if (Getter)
            sb.Append("get ");

        if (Setter)
            sb.Append("set ");

        if (TypeName is not null)
        {
            sb.Append(TypeName);

            if (Name is not null)
                sb.Append('.');
        }

        if (Name is not null)
            sb.Append(Name);

        if (TargetTypeName is not null)
        {
            sb.Append(" as ");
            sb.Append(TargetTypeName);
            Body?.ToString(sb);
            return;
        }

        if (IsIndexer)
            sb.Append('[');
        else if (Name is not null || Parameters.Count > 1)
            sb.Append('(');

        Parameters.ToString(sb);

        if (IsIndexer)
            sb.Append(']');
        else if (Name is not null || Parameters.Count > 1)
            sb.Append(") ");

        if (Name is null)
            sb.Append(" => ");

        Body?.ToString(sb);
    }
}
