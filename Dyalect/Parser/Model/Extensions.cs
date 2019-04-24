using Dyalect.Compiler;
using Dyalect.Parser.Model;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Parser.Model
{
    internal static class Extensions
    {
        public static void ToString(this IEnumerable<DNode> nodes, StringBuilder sb, string sep = ",")
        {
            var fst = true;

            foreach (var n in nodes)
            {
                if (!fst)
                    sb.Append(sep + " ");

                n.ToString(sb);
                fst = false;
            }
        }

        public static string ToSymbol(this BinaryOperator op)
        {
            switch (op)
            {
                case BinaryOperator.Add: return Builtins.Add;
                case BinaryOperator.And: return Builtins.BoolAnd;
                case BinaryOperator.Div: return Builtins.Div;
                case BinaryOperator.Eq: return Builtins.Eq;
                case BinaryOperator.Gt: return Builtins.Gt;
                case BinaryOperator.GtEq: return Builtins.Gte;
                case BinaryOperator.Lt: return Builtins.Lt;
                case BinaryOperator.LtEq: return Builtins.Lte;
                case BinaryOperator.Mul: return Builtins.Mul;
                case BinaryOperator.NotEq: return Builtins.Neq;
                case BinaryOperator.Or: return Builtins.BoolOr;
                case BinaryOperator.Rem: return Builtins.Rem;
                case BinaryOperator.Sub: return Builtins.Sub;
                case BinaryOperator.BitwiseAnd: return Builtins.And;
                case BinaryOperator.BitwiseOr: return Builtins.Or;
                case BinaryOperator.Xor: return Builtins.Xor;
                case BinaryOperator.ShiftLeft: return Builtins.Shl;
                case BinaryOperator.ShiftRight: return Builtins.Shr;
                default: return "";
            }
        }

        public static string ToSymbol(this UnaryOperator op)
        {
            switch (op)
            {
                case UnaryOperator.Neg: return Builtins.Neg;
                case UnaryOperator.Not: return Builtins.Not;
                case UnaryOperator.BitwiseNot: return Builtins.BitNot;
                case UnaryOperator.Plus: return Builtins.Plus;
                default: return "";
            }
        }
    }
}
