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
        Break,
        Continue,
        Return,
        Yield,

        Base,

        Application,
        Index,
        Access,

        Function,
        Tuple,
        Array,
        Label,

        Type,
        Field
    }

}
