namespace Dyalect.Compiler
{
    internal static class Builtins
    {
        public const string Add         = "op_add";
        public const string Sub         = "op_sub";
        public const string Mul         = "op_mul";
        public const string Div         = "op_div";
        public const string Rem         = "op_rem";
        public const string Shl         = "op_shl";
        public const string Shr         = "op_shr";
        public const string And         = "op_and";
        public const string Or          = "op_or";
        public const string Xor         = "op_xor";
        public const string Eq          = "op_eq";
        public const string Neq         = "op_neq";
        public const string Gt          = "op_gt";
        public const string Lt          = "op_lt";
        public const string Gte         = "op_gte";
        public const string Lte         = "op_lte";
        public const string Neg         = "op_negate";
        public const string Plus        = "op_plus";
        public const string Not         = "op_not";
        public const string BitNot      = "op_bitcomp";
        public const string Get         = "op_get";
        public const string Set         = "op_set";

        public const string Len         = "len";
        public const string ToStr       = "toString";
        public const string Iterator    = "iter";
        public const string Clone       = "clone";
        public const string Has         = "has";
        public const string Type        = "getType";
        public const string Call        = "call";
    }
}
