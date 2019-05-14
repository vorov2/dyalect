using Dyalect.Parser.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Linker
{
    internal static class LiteralEvaluator
    {
        public static void Eval(DNode node)
        {
            switch (node.NodeType)
            {
                default:
                    throw new DyException($"Node of type {node.NodeType} is not supported.");
            }
        }
    }
}
