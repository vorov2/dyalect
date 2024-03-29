﻿namespace Dyalect.Parser.Model;

public enum NodeType
{
    None,
    Block,
    PrivateScope,

    TestBlock,

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
    As,
    Variant,

    Directive,

    Base,

    Application,
    Index,
    Access,
    
    Function,
    Tuple,
    Array,
    Iterator,
    YieldBlock,
    Label,
    Parameter,

    RecursiveBlock,
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
    NotPattern,
    ComparisonPattern,
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

