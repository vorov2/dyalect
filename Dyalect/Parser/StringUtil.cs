using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dyalect.Parser
{
    internal static class StringUtil
    {
        private static readonly Dictionary<string, string> replaceDict = new Dictionary<string, string>
        {
            { "\a", @"\a" },
            { "\b", @"\b" },
            { "\f", @"\f" },
            { "\n", @"\n" },
            { "\r", @"\r" },
            { "\t", @"\t" },
            { "\v", @"\v" },
            { "\\", @"\\" },
            { "\0", @"\0" }
        };

        private const string regexEscapes = @"[\a\b\f\n\r\t\v\\""]";

        public static string Escape(string value)
        {
            return "\"" + Regex.Replace(value, regexEscapes, Match) + "\"";
        }

        private static string Match(Match m)
        {
            string match = m.ToString();

            if (replaceDict.ContainsKey(match))
                return replaceDict[match];

            return string.Empty;
        }
    }
}
