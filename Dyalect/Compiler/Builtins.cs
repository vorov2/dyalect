namespace Dyalect.Compiler
{
    internal static class Builtins
    {
        private const string SET = "set_";
        public static string Setter(string name) => $"{SET}{name}";
        public static bool IsSetter(string name) => name.StartsWith(SET);
        public static string GetSetterName(string name) => name[SET.Length..];
        public static string Translate(string op) =>
            op switch
            {
                Add => "+",
                Sub => "-",
                Mul => "*",
                Div => "/",
                Rem => "%",
                Shl => "<<<",
                Shr => ">>>",
                And => "&&",
                Or => "||",
                Eq => "==",
                Neq => "!=",
                Gt => ">",
                Lt => "<",
                Gte => ">=",
                Lte => "<=",
                Not => "!",
                Xor => "^^^",
                BitNot => "~~~",
                BitAnd => "&&&",
                BitOr => "|||",
                _ => op
            };

        public const string OperatorSymbols = "?:+-*/%<>^=!~|";

        public const string Add         = "__op_add";
        public const string Sub         = "__op_sub";
        public const string Mul         = "__op_mul";
        public const string Div         = "__op_div";
        public const string Rem         = "__op_rem";
        public const string Shl         = "__op_shl";
        public const string Shr         = "__op_shr";
        public const string And         = "__op_and";
        public const string Or          = "__op_or";
        public const string Xor         = "__op_xor";
        public const string Eq          = "__op_eq";
        public const string Neq         = "__op_neq";
        public const string Gt          = "__op_gt";
        public const string Lt          = "__op_lt";
        public const string Gte         = "__op_gte";
        public const string Lte         = "__op_lte";
        public const string Neg         = "__op_negate";
        public const string Plus        = "__op_plus";
        public const string Not         = "__op_not";
        public const string BitNot      = "__op_bitcomp";
        public const string BitAnd      = "__op_bitand";
        public const string BitOr       = "__op_bitor";
        public const string Get         = "__op_get";
        public const string Set         = "__op_set";
        public const string Len         = "Length";
        public const string ToStr       = "ToString";
        public const string ToLit       = "ToLiteral";
        public const string ToTuple     = "ToTuple";
        public const string ToArray     = "ToArray";
        public const string Iterator    = "Iterate";
        public const string Clone       = "Clone";
        public const string Has         = "Has";
        public const string Type        = "GetType";
        public const string Call        = "Call";
        public const string Range       = "Range";
        public const string Slice       = "Slice";
        public const string Concat      = "Concat";
        public const string Dispose     = "Dispose";
        public const string Contains    = "Contains";
        public const string DelMember   = "DeleteMember";
    }
}
