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
            NodeType.Boolean => ((DBooleanLiteral)node).Value ? True : False,
            NodeType.Char => new DyChar(((DCharLiteral)node).Value),
            NodeType.String => GetStringValue((DStringLiteral)node),
            NodeType.Integer => DyInteger.Get(((DIntegerLiteral)node).Value),
            NodeType.Float => new DyFloat(((DFloatLiteral)node).Value),
            NodeType.Tuple => new DyArray(GetArray(((DTupleLiteral)node).Elements, out _)),
            NodeType.Array => ProcessArrayLiteral((DArrayLiteral)node),
            NodeType.Variant => ProcessVariant((DVariant)node),
            _ => throw new DyCodeException(DyError.ParsingFailed, $"Node of type \"{node.NodeType}\" is not supported.")
        };

    private static DyObject ProcessArrayLiteral(DArrayLiteral node)
    {
        var arr = GetArray(node.Elements, out var hasLabels);

        if (!hasLabels)
            return new DyArray(arr);

        var dict = new Dictionary<DyObject, DyObject>();

        foreach (var v in arr)
        {
            if (v is DyLabel lab)
                dict.Add(new DyString(lab.Label), lab.Value);
            else
                throw new DyCodeException(DyError.ParsingFailed, $"Invalid dictionary literal.");
        }

        return new DyDictionary(dict);
    }

    private static DyObject ProcessVariant(DVariant node) =>
        new DyVariant(node.Name, new DyTuple(GetArray(node.Arguments, out _)));

    private static DyObject GetStringValue(DStringLiteral lit)
    {
        if (lit.Chunks is not null || lit.Value is null)
            throw new DyCodeException(new DyVariant(DyError.ParsingFailed, $"Interpolated strings are not supported."));

        return DyString.Get(lit.Value);
    }

    private static DyObject[] GetArray(List<DNode> nodes, out bool hasLabels)
    {
        var arr = new DyObject[nodes.Count];
        hasLabels = false;

        for (var i = 0; i < nodes.Count; i++)
        {
            var e = nodes[i];
            DyObject obj;

            if (e.NodeType == NodeType.Label)
            {
                var lab = (DLabelLiteral)e;
                obj = new DyLabel(lab.Label, Eval(lab.Expression));
                hasLabels = true;
            }
            else
                obj = Eval(e);

            arr[i] = obj;
        }

        return arr;
    }
}
