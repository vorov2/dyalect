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
        Mixin,

        Function,
        Import,
        Tuple,
        NameTag,

        Type,
        Field
    }

}
