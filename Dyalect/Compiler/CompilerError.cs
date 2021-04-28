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

        CtorOnlyMethod = 210,

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

        CtorOnlyLocalType = 229,

        PrivateMethod = 230,

        CtorAutoGen = 231,

        UnableToLinkModule = 232,

        BindingPatternNoInit = 233,

        InvalidLabel = 234,

        UnknownDirective = 235,

        InvalidDirective = 236,
        
        InvalidSlice = 237,

        InvalidRethrow = 238,

        CtorNotIterator = 239,

        PositionalArgumentAfterKeyword = 240,

        CtorOnlyStatic = 241,

        CtorOnlyPascal = 242,

        // = 243
    }
}
