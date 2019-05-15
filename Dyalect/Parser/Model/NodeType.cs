namespace Dyalect.Parser.Model
{
    public enum NodeType
    {
        None,
        Block,

        Integer,
        Float,
        String,
        Char,
        Boolean,
        Nil,

        Name,
        Binary,
        Unary,
        Assignment,
        Binding,
        If,
        While,
        For,
        Range,
        Break,
        Continue,
        Return,
        Yield,

        Base,

        Application,
        Index,
        Access,
        MemberCheck,

        Function,
        Tuple,
        Array,
        Label,
        Parameter,

        Type,
        Field
    }

}
