namespace Dyalect.Compiler;

internal static class Builtins
{
    private const string SET = "set_";
    private const string GET = "get_";
    public static string Setter(string name) => $"{SET}{name}";
    public static string Getter(string name) => $"{GET}{name}";
    public static bool IsSetter(string name) => name.StartsWith(SET);
    public static string GetSetterName(string name) => name[SET.Length..];
    public static bool IsSetter(HashString name) => ((string)name).StartsWith(SET);
    public static string GetSetterName(HashString name) => ((string)name)[SET.Length..];
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
            Neg => "- (unary)",
            Plus => "+ (unary)",
            _ => op
        };

    public const string OperatorSymbols = "?:+-*/&%<>^=!~|";

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
    public const string BitAnd      = "op_bitand";
    public const string BitOr       = "op_bitor";
    public const string Get         = "op_get";
    public const string Set         = "op_set";
    public const string Length         = "Length";
    public const string ToString       = "ToString";
    public const string ToLit       = "ToLiteral";
    public const string ToTuple     = "ToTuple";
    public const string ToArray     = "ToArray";
    public const string Iterator    = "Iterate";
    public const string Clone       = "Clone";
    public const string Max         = "Max";
    public const string Min         = "Min";
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
