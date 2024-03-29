﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dyalect.Parser;

public static class StringUtil
{
    private static readonly Dictionary<string, string> replaceDict =
        new()
        {
            { "\a", @"\a" },
            { "\b", @"\b" },
            { "\f", @"\f" },
            { "\n", @"\n" },
            { "\r", @"\r" },
            { "\t", @"\t" },
            { "\v", @"\v" },
            { "\\", @"\\" },
            { "\0", @"\0" },
            { "\"", @"\""" }
        };

    private const string regexEscapes = @"[\a\b\f\n\r\t\v\\""]";

    public static string Escape(string value, string quote = "\"") =>
        quote + Regex.Replace(value, regexEscapes, Match) + quote;

    private static string Match(Match m)
    {
        var match = m.ToString();

        if (replaceDict.ContainsKey(match))
            return replaceDict[match];

        return string.Empty;
    }
}
