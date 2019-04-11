namespace Dyalect.Parser.Model
{
    public enum NodeType
    {
        None,
        Block,

        Integer,
        Float,
        String,
        Boolean,
        Nil,

        Name,
        Binary,
        Unary,
        Assignment,
        Binding,
        If,
        While,
        Break,
        Continue,
        Return,

        Application,
        Index,

        Function,
        Import,
        Tuple,
        Array,
        NameTag,

        Type,
        Field
    }

}
