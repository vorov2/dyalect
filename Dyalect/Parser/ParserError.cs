﻿namespace Dyalect.Parser;

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

    InvalidSyntax = 17,

    InvalidCharLiteral = 18,

    InvalidTryCatch = 19,

    InvalidLabel = 20,

    InvalidImport = 21,

    InvalidApplicationArguments = 22,
    
    Deprecated = 23,

    InvalidTypeName = 34,

    InvalidRegion = 35,

    InvalidFunction = 36,

    InvalidYield = 37,

    InvalidRange = 38,

    InvalidIdentifier = 39,

    InvalidApplicationOperator = 40,

    InvalidIndex = 41,

    ExpectedFunction = 42,

    InvalidGuardedStatement = 43,

    InvalidNumber = 44,

    AbstractFunctionNoBody = 45
}
