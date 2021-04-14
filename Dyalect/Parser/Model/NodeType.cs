namespace Dyalect.Parser.Model
{
    public enum NodeType
    {
        None,
        Block,
        PrivateScope,

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
        Rebinding,
        If,
        While,
        For,
        Range,
        Break,
        Continue,
        Return,
        Yield,
        YieldBreak,
        YieldMany,
        Throw,

        Directive,

        Base,

        Application,
        Index,
        Access,
        MemberCheck,

        Function,
        Tuple,
        Array,
        Iterator,
        YieldBlock,
        Label,
        Parameter,

        Type,
        Field,

        Match,
        MatchEntry,
        NamePattern,
        IntegerPattern,
        FloatPattern,
        BooleanPattern,
        CharPattern,
        StringPattern,
        NilPattern,
        ArrayPattern,
        RangePattern,
        TuplePattern,
        LabelPattern,
        AsPattern,
        WildcardPattern,
        TypeTestPattern,
        AndPattern,
        OrPattern,
        MethodCheckPattern,
        CtorPattern,

        TryCatch
    }

}
