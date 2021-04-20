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

        public static string ToSymbol(this BinaryOperator op) =>
            op switch
            {
                BinaryOperator.Add => "+",
                BinaryOperator.And => "&&",
                BinaryOperator.Div => "/",
                BinaryOperator.Eq => "==",
                BinaryOperator.Gt => ">",
                BinaryOperator.GtEq => ">=",
                BinaryOperator.Lt => "<",
                BinaryOperator.LtEq => "<=",
                BinaryOperator.Mul => "*",
                BinaryOperator.NotEq => "!=",
                BinaryOperator.Or => "||",
                BinaryOperator.Rem => "%",
                BinaryOperator.Sub => "-",
                BinaryOperator.BitwiseAnd => "&",
                BinaryOperator.BitwiseOr => "|",
                BinaryOperator.Xor => "^",
                BinaryOperator.ShiftLeft => "<<<",
                BinaryOperator.ShiftRight => ">>>",
                BinaryOperator.Coalesce => "??",
                BinaryOperator.Is => "is",
                _ => "",
            };

        public static string ToSymbol(this UnaryOperator op) =>
            op switch
            {
                UnaryOperator.Neg => "-",
                UnaryOperator.Not => "!",
                UnaryOperator.BitwiseNot => "~",
                UnaryOperator.Plus => "+",
                _ => "",
            };
    }
}
