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
        Rebinding,
        If,
        While,
        For,
        Range,
        Break,
        Continue,
        Return,
        Yield,
        Throw,

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
        RecordPattern,
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
