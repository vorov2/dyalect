using System.Collections.Generic;
using System.Text;
namespace Dyalect.Parser.Model;

public sealed class DRecursiveBlock : DNode
{
    public DRecursiveBlock(Location loc) : base(NodeType.RecursiveBlock, loc) { }

    public List<DFunctionDeclaration> Functions { get; } = new();

    internal override void ToString(StringBuilder sb)
    {
        for (var i = 0; i < Functions.Count; i++)
            FunctionToString(i == 0 ? "func " : " and ", Functions[0], sb);
    }

    private void FunctionToString(string prefix, DFunctionDeclaration func, StringBuilder sb)
    {
        sb.Append(prefix);
        sb.Append('(');
        func.Parameters.ToString(sb);
        sb.Append(") ");
        func.Body?.ToString(sb);
    }
}
