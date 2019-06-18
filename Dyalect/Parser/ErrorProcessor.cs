using Dyalect.Strings;
using System;
using System.Collections.Generic;
using static Dyalect.Parser.ParserError;

namespace Dyalect.Parser
{
    internal static class ErrorProcessor
    {
        private static Dictionary<string, ParserError> errors =
            new Dictionary<string, ParserError>
            {
                 { "invalid StandardOperators", InvalidStandardOperators }
                ,{ "invalid FunctionName", InvalidFunctionName}
                ,{ "invalid Statement", InvalidStatement }
                ,{ "invalid Binding", InvalidBinding }
                ,{ "invalid SimpleExpr", InvalidExpression }
                ,{ "invalid ControlFlow", InvalidStatement }
                ,{ "invalid If", InvalidIf }
                ,{ "invalid Loops", InvalidStatement }
                ,{ "invalid Expr", InvalidExpression }
                ,{ "invalid FunctionExpr", InvalidFunctionExpression }
                ,{ "invalid Unary", InvalidUnary }
                ,{ "invalid Bool", InvalidLiteral }
                ,{ "invalid Literal", InvalidLiteral }
                ,{ "invalid DyalectItem", InvalidStatement }
                ,{ "invalid Pattern", InvalidPattern }
                ,{ "invalid BooleanPattern", InvalidPattern }
                ,{ "invalid TryCatch", InvalidTryCatch }
                ,{ "??? expected", Undefined }
            };

        private static Dictionary<string, string> tokens =
            new Dictionary<string, string>
            {
                 { "identToken", "identifier" }
                ,{ "intToken", "integer literal" }
                ,{ "floatToken", "float literal" }
                ,{ "stringToken", "string literal" }
                ,{ "charToken", "char literal" }
                ,{ "varToken", "var" }
                ,{ "constToken", "const" }
                ,{ "funcToken", "func" }
                ,{ "returnToken", "return" }
                ,{ "continueToken", "continue" }
                ,{ "breakToken", "break" }
                ,{ "yieldToken", "yield" }
                ,{ "ifToken", "if" }
                ,{ "forToken", "for" }
                ,{ "whileToken", "while" }
                ,{ "typeToken", "type" }
                ,{ "arrowToken", "=>" }
                ,{ "doToken", "do" }
                ,{ "dotToken", "." }
                ,{ "commaToken", "," }
                ,{ "semicolonToken", ";" }
                ,{ "colonToken", ":" }
                ,{ "equalToken", "==" }
                ,{ "parenLeftToken", "(" }
                ,{ "parenRightToken", ")" }
                ,{ "curlyLeftToken", "{" }
                ,{ "curlyRightToken", "}" }
                ,{ "squareLeftToken", "[" }
                ,{ "squareRightToken", "]" }
                ,{ "implicitToken", "implicit" }
                ,{ "minus", "-" }
                ,{ "plus", "+" }
                ,{ "not", "!" }
                ,{ "bitnot", "~" }
            };

        public static void ProcessError(string source, out string detail, out ParserError error)
        {
            if (errors.TryGetValue(source, out error))
                detail = ParserErrors.ResourceManager.GetString(error.ToString());
            else
            {
                var twoParts = source.Split(new char[] { '\u0020' }, StringSplitOptions.RemoveEmptyEntries);

                if (twoParts.Length != 2)
                {
                    error = Undefined;
                    detail = ParserErrors.Undefined;
                    return;
                }

                var token = twoParts[0].Trim('\"');

                if (tokens.TryGetValue(token, out var nt))
                    token = nt;
                else if (token == "invalid")
                {
                    error = InvalidSyntax;
                    detail = string.Format(ParserErrors.InvalidSyntax, source);
                    return;
                }

                error = TokenExpected;
                detail = string.Format(ParserErrors.TokenExpected, token);
            }
        }
    }
}
