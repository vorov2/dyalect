using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using static Dyalect.Parser.ParserError;

namespace Dyalect.Parser
{
    internal static class ErrorProcessor
    {
        private static readonly Dictionary<string, ParserError> errors =
            new()
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
                ,{ "invalid Index", InvalidExpression }
                ,{ "invalid SimpleIndex", InvalidExpression }
                ,{ "invalid IndexBody", InvalidExpression }
                ,{ "invalid FunctionExpr", InvalidFunctionExpression }
                ,{ "invalid Unary", InvalidUnary }
                ,{ "invalid Bool", InvalidLiteral }
                ,{ "invalid String", InvalidLiteral }
                ,{ "invalid Tuple", InvalidLiteral }
                ,{ "invalid Literal", InvalidLiteral }
                ,{ "invalid SimpleLiteral", InvalidLiteral }
                ,{ "invalid DyalectItem", InvalidStatement }
                ,{ "invalid Pattern", InvalidPattern }
                ,{ "invalid BooleanPattern", InvalidPattern }
                ,{ "invalid CtorPattern", InvalidPattern }
                ,{ "invalid TuplePattern", InvalidPattern }
                ,{ "invalid TryCatch", InvalidTryCatch }
                ,{ "invalid Label", InvalidLabel }
                ,{ "invalid Import", InvalidImport }
                ,{ "invalid ApplicationArguments", InvalidApplicationArguments }
                ,{ "??? expected", Undefined }
            };

        private static readonly Dictionary<string, string> tokens =
            new()
            {
                 { "identToken", "identifier" }
                ,{ "directive", "compiler directive" }
                ,{ "intToken", "integer literal" }
                ,{ "floatToken", "float literal" }
                ,{ "stringToken", "string literal" }
                ,{ "verbatimStringToken", "multiline string literal" }
                ,{ "charToken", "char literal" }
                ,{ "implicitToken", "implicit" }
                ,{ "privateToken", "private" }
                ,{ "inToken", "in" }
                ,{ "autoToken", "auto" }
                ,{ "varToken", "var" }
                ,{ "autoToken", "auto" }
                ,{ "letToken", "let" }
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
                ,{ "eq_coa", "??=" }
                ,{ "eq_add", "+=" }
                ,{ "eq_sub", "-=" }
                ,{ "eq_mul", "*=" }
                ,{ "eq_div", "/=" }
                ,{ "eq_rem", "%=" }
                ,{ "eq_and", "&&&=" }
                ,{ "eq_or", "|||=" }
                ,{ "eq_xor", "^=" }
                ,{ "eq_lsh", "<<<=" }
                ,{ "eq_rsh", ">>>=" }
                ,{ "minus", "-" }
                ,{ "plus", "+" }
                ,{ "not", "!" }
                ,{ "bitnot", "~~~" }
                ,{ "coalesce", "??" }
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

                if (!token.All(c => char.IsLetter(c)))
                    token = "\"" + token + "\"";

                error = TokenExpected;
                detail = string.Format(ParserErrors.TokenExpected, token);
            }
        }
    }
}
