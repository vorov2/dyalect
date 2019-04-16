using Dyalect.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect
{
    internal static class Config
    {
        public static ConsoleColor Foreground { get; set; }

        public static ConsoleColor Background { get; set; }

        public static ConsoleColor Prefix { get; set; }

        public static ConsoleColor Error { get; set; }

        public static ConsoleColor Warning { get; set; }

        public static ConsoleColor Info { get; set; }

        public static ConsoleColor Header { get; set; }

        public static ConsoleColor Subheader { get; set; }

        public static ConsoleColor Output { get; set; }

        public static ConsoleColor SupplementaryOutput { get; set; }

        public static void SetDefault()
        {
            Foreground = ConsoleColor.Gray;
            Background = ConsoleColor.Black;
            Header = ConsoleColor.Gray;
            Subheader = ConsoleColor.Gray;
            Output = ConsoleColor.Gray;
            SupplementaryOutput = ConsoleColor.Gray;
            Error = ConsoleColor.Gray;
            Warning = ConsoleColor.Gray;
            Info = ConsoleColor.Gray;
            Prefix = ConsoleColor.Gray;
        }

        public static void Read(IDictionary<string, object> dict)
        {
            foreach (var kv in dict)
            {
                switch (kv.Key.ToLower())
                {
                    case "foreground": Foreground = Parse(kv.Value); break;
                    case "background": Background = Parse(kv.Value); break;
                    case "error": Error = Parse(kv.Value); break;
                    case "warning": Warning = Parse(kv.Value); break;
                    case "info": Info = Parse(kv.Value); break;
                    case "output": Output = Parse(kv.Value); break;
                    case "header": Header = Parse(kv.Value); break;
                    case "subheader": Subheader = Parse(kv.Value); break;
                    case "supplementaryoutput": SupplementaryOutput = Parse(kv.Value); break;
                    case "prefix": Prefix = Parse(kv.Value); break;
                    default:
                        throw new Exception($"Unknown color code {kv.Key}.");
                }
            }
        }

        private static ConsoleColor Parse(object value)
        {
            if (!(value is string str) || !Enum.TryParse<ConsoleColor>(str, true, out var col))
                throw new Exception($"Invalid value {value} for a color.");
            return col;
        }
    }
}
