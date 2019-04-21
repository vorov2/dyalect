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
                case BinaryOperator.Add: return Traits.AddName;
                case BinaryOperator.And: return Traits.BoolAndName;
                case BinaryOperator.Div: return Traits.DivName;
                case BinaryOperator.Eq: return Traits.EqName;
                case BinaryOperator.Gt: return Traits.GtName;
                case BinaryOperator.GtEq: return Traits.GteName;
                case BinaryOperator.Lt: return Traits.LtName;
                case BinaryOperator.LtEq: return Traits.LteName;
                case BinaryOperator.Mul: return Traits.MulName;
                case BinaryOperator.NotEq: return Traits.NeqName;
                case BinaryOperator.Or: return Traits.BoolOrName;
                case BinaryOperator.Rem: return Traits.RemName;
                case BinaryOperator.Sub: return Traits.SubName;
                case BinaryOperator.BitwiseAnd: return Traits.AndName;
                case BinaryOperator.BitwiseOr: return Traits.OrName;
                case BinaryOperator.Xor: return Traits.XorName;
                case BinaryOperator.ShiftLeft: return Traits.ShlName;
                case BinaryOperator.ShiftRight: return Traits.ShrName;
                default: return "";
            }
        }

        public static string ToSymbol(this UnaryOperator op)
        {
            switch (op)
            {
                case UnaryOperator.Neg: return Traits.NegName;
                case UnaryOperator.Not: return Traits.NotName;
                case UnaryOperator.BitwiseNot: return Traits.BitName;
                case UnaryOperator.Length: return Traits.LenName;
                case UnaryOperator.Plus: return Traits.PlusName;
                default: return "";
            }
        }
    }
}
