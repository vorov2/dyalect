using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Linker
{
    internal static class LiteralEvaluator
    {
        public static DyObject Eval(DNode node) =>
            node.NodeType switch
            {
                NodeType.String => new DyString(((DStringLiteral)node).Value),
                NodeType.Integer => ctx.RuntimeContext.Integer.Get(((DIntegerLiteral)node).Value),
                NodeType.Float => new DyFloat(((DFloatLiteral)node).Value),
                NodeType.Char => new DyChar(((DCharLiteral)node).Value),
                NodeType.Boolean => ((DBooleanLiteral)node).Value ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False,
                NodeType.Nil => ctx.RuntimeContext.Nil.Instance,
                NodeType.Tuple => new DyTuple(GetArray(((DTupleLiteral)node).Elements, allowLabels: true)),
                NodeType.Array => new DyArray(GetArray(((DArrayLiteral)node).Elements, allowLabels: false)),
                _ => throw new DyException($"Node of type {node.NodeType} is not supported."),
            };

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
}
