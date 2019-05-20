namespace Dyalect.Parser
{
    public enum ParserError
    {
        None = 0,

        Undefined = 1,

        TokenExpected = 2,

        InvalidStandardOperators = 3,

        InvalidFunctionName = 4,

        InvalidStatement = 5,

        InvalidBinding = 6,

        InvalidExpression = 7,

        InvalidIf = 8,

        InvalidFunctionExpression = 9,

        InvalidUnary = 10,

        InvalidLiteral = 11,

        CodeIslandsNotAllowed = 12,

        CodeIslandInvalid = 13,

        InvalidEscapeCode = 14,

        SemanticError = 15,

        InvalidPattern = 16,
    }
}
