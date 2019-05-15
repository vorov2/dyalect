using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect.Linker
{
    internal static class LiteralEvaluator
    {
        public static DyObject Eval(DNode node)
        {
            switch (node.NodeType)
            {
                case NodeType.String:
                    return new DyString(((DStringLiteral)node).Value);
                case NodeType.Integer:
                    return DyInteger.Get(((DIntegerLiteral)node).Value);
                case NodeType.Float:
                    return new DyFloat(((DFloatLiteral)node).Value);
                case NodeType.Char:
                    return new DyChar(((DCharLiteral)node).Value);
                case NodeType.Boolean:
                    return ((DBooleanLiteral)node).Value ? DyBool.True : DyBool.False;
                case NodeType.Nil:
                    return DyNil.Instance;
                case NodeType.Tuple:
                    {
                        var t = (DTupleLiteral)node;
                        return new DyTuple(GetArray(t.Elements, allowLabels: true));
                    }
                case NodeType.Array:
                    {
                        var t = (DArrayLiteral)node;
                        return new DyArray(GetArray(t.Elements, allowLabels: false));
                    }
                default:
                    throw new DyException($"Node of type {node.NodeType} is not supported.");
            }
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
}
