namespace Dyalect.Compiler
{
    public enum CompilerError
    {
        None = 0,

        TooManyErrors = 201,

        VariableAlreadyDeclared = 202,

        UndefinedVariable = 203,

        UnableAssignExpression = 204,

        UnableAssignConstant = 205,

        NoEnclosingLoop = 206,

        UndefinedType = 207,

        UndefinedModule = 208,

        ReturnNotAllowed = 209,

        NestedMethod = 210,

        ExpressionNoName = 211,

        PrivateNameAccess = 212,

        UndefinedBaseVariable = 213,

        BaseNotAllowed = 214,

        StaticOnlyMethods = 215,

        ReturnInIterator = 216,

        CodeIslandEmpty = 217,

        CodeIslandInvalid = 218,

        VarArgNoDefaultValue = 219,

        VarArgOnlyOne = 220,

        InvalidDefaultValue = 221,

        PatternNotSupported = 222,

        SliceNotSupported = 223,

        NamedArgumentMultipleTimes = 224,

        OverrideNotAllowed = 225,

        TypeAlreadyDeclared = 226,

        PrivateScopeOnlyGlobal = 227,

        PrivateScopeNested = 228,

        MemberNameCamel = 229,

        PrivateMethod = 230,

        TypesOnlyGlobalScope = 231,

        UnableToLinkModule = 232,

        BindingPatternNoInit = 233,

        InvalidLabel = 234,

        UnknownDirective = 235,

        InvalidDirective = 236,
        
        InvalidSlice = 237,

        InvalidRethrow = 238,

        LabelOnlyCamel = 239,

        PositionalArgumentAfterKeyword = 240,

        IndexerStatic = 241,

        IndexerWrongArguments = 242,

        IndexerSetOrGet = 243,

        TypeNameCamel = 244,

        AccessorOnlyMethod = 245,

        AutoNotAllowed = 246,

        InvalidTypeDefaultValue = 247,

        GuardOnBinding = 248,

        MethodNotRecursive = 249,

        DuplicateModuleAlias = 250,

        InvalidLazyBinding = 251,

        InvalidCast = 252,

        YieldNotAllowed = 253,

        BoolCastNotAllowed = 254,

        SelfCastNotAllowed = 255,

        InvalidFunctionArgument = 256
    }
}
