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
                case BinaryOperator.Add: return BuiltinMixins.AddName;
                case BinaryOperator.And: return BuiltinMixins.BoolAndName;
                case BinaryOperator.Div: return BuiltinMixins.DivName;
                case BinaryOperator.Eq: return BuiltinMixins.EqName;
                case BinaryOperator.Gt: return BuiltinMixins.GtName;
                case BinaryOperator.GtEq: return BuiltinMixins.GteName;
                case BinaryOperator.Lt: return BuiltinMixins.LtName;
                case BinaryOperator.LtEq: return BuiltinMixins.LteName;
                case BinaryOperator.Mul: return BuiltinMixins.MulName;
                case BinaryOperator.NotEq: return BuiltinMixins.NeqName;
                case BinaryOperator.Or: return BuiltinMixins.BoolOrName;
                case BinaryOperator.Rem: return BuiltinMixins.RemName;
                case BinaryOperator.Sub: return BuiltinMixins.SubName;
                case BinaryOperator.BitwiseAnd: return BuiltinMixins.AndName;
                case BinaryOperator.BitwiseOr: return BuiltinMixins.OrName;
                case BinaryOperator.Xor: return BuiltinMixins.XorName;
                case BinaryOperator.ShiftLeft: return BuiltinMixins.ShlName;
                case BinaryOperator.ShiftRight: return BuiltinMixins.ShrName;
                default: return "";
            }
        }

        public static string ToSymbol(this UnaryOperator op)
        {
            switch (op)
            {
                case UnaryOperator.Neg: return BuiltinMixins.NegName;
                case UnaryOperator.Not: return BuiltinMixins.NotName;
                case UnaryOperator.BitwiseNot: return BuiltinMixins.BitName;
                case UnaryOperator.Length: return BuiltinMixins.LenName;
                default: return "";
            }
        }
    }
}
