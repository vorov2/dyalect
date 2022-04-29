using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
using Dyalect.Runtime;
using System.Collections.Generic;
namespace Dyalect.Linker;

internal static class LiteralEvaluator
{
    public static DyObject Eval(DNode node) =>
        node.NodeType switch
        {
            NodeType.Nil => Nil,
            NodeType.String => GetStringValue((DStringLiteral)node),
            NodeType.Integer => DyInteger.Get(((DIntegerLiteral)node).Value),
            NodeType.Float => new DyFloat(((DFloatLiteral)node).Value),
            NodeType.Char => new DyChar(((DCharLiteral)node).Value),
            NodeType.Boolean => ((DBooleanLiteral)node).Value ? True : False,
            NodeType.Tuple => new DyTuple(GetArray(((DTupleLiteral)node).Elements, allowLabels: true)),
            NodeType.Array => new DyArray(GetArray(((DArrayLiteral)node).Elements, allowLabels: false)),
            _ => new DyVariant(DyErrorCode.ParsingFailed, $"Node of type \"{node.NodeType}\" is not supported.")
        };

    private static DyObject GetStringValue(DStringLiteral lit)
    {
        if (lit.Chunks is not null || lit.Value is null)
            throw new DyCodeException(new DyVariant(DyErrorCode.ParsingFailed, $"Interpolated strings are not supported."));

        return DyString.Get(lit.Value);
    }

    private static DyObject[] GetArray(List<DNode> nodes, bool allowLabels)
    {
        var arr = new DyObject[nodes.Count];

        for (var i = 0; i < nodes.Count; i++)
        {
            var e = nodes[i];
            DyObject obj;

            if (allowLabels && e.NodeType == NodeType.Label)
            {
                var lab = (DLabelLiteral)e;
                obj = new DyLabel(lab.Label, Eval(lab.Expression));
            }
            else
                obj = Eval(e);

            arr[i] = obj;
        }

        return arr;
    }
}
