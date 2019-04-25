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
        For,
        Break,
        Continue,
        Return,
        Yield,

        Base,
        This,

        Application,
        Index,
        Trait,

        Function,
        Import,
        Tuple,
        Array,
        Label,

        Type,
        Field
    }

}
