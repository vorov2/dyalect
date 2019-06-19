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
                case BinaryOperator.Add:        return "+";
                case BinaryOperator.And:        return "&&";
                case BinaryOperator.Div:        return "/";
                case BinaryOperator.Eq:         return "==";
                case BinaryOperator.Gt:         return ">";
                case BinaryOperator.GtEq:       return ">=";
                case BinaryOperator.Lt:         return "<";
                case BinaryOperator.LtEq:       return "<=";
                case BinaryOperator.Mul:        return "*";
                case BinaryOperator.NotEq:      return "!=";
                case BinaryOperator.Or:         return "||";
                case BinaryOperator.Rem:        return "%";
                case BinaryOperator.Sub:        return "-";
                case BinaryOperator.BitwiseAnd: return "&";
                case BinaryOperator.BitwiseOr:  return "|";
                case BinaryOperator.Xor:        return "^";
                case BinaryOperator.ShiftLeft:  return "<<";
                case BinaryOperator.ShiftRight: return ">>";
                case BinaryOperator.Coalesce:   return "??";
                case BinaryOperator.Is:         return "is";
                default: return "";
            }
        }

        public static string ToSymbol(this UnaryOperator op)
        {
            switch (op)
            {
                case UnaryOperator.Neg:         return "-";
                case UnaryOperator.Not:         return "!";
                case UnaryOperator.BitwiseNot:  return "~";
                case UnaryOperator.Plus:        return "+";
                default: return "";
            }
        }
    }
}
