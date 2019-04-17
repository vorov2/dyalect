using System;
using System.Collections.Generic;

namespace Dyalect
{
    internal static class Theme
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
                    case "colors.foreground": Foreground = Parse(kv.Value); break;
                    case "colors.background": Background = Parse(kv.Value); break;
                    case "colors.error": Error = Parse(kv.Value); break;
                    case "colors.warning": Warning = Parse(kv.Value); break;
                    case "colors.info": Info = Parse(kv.Value); break;
                    case "colors.output": Output = Parse(kv.Value); break;
                    case "colors.header": Header = Parse(kv.Value); break;
                    case "colors.subheader": Subheader = Parse(kv.Value); break;
                    case "colors.supplementary": SupplementaryOutput = Parse(kv.Value); break;
                    case "colors.prefix": Prefix = Parse(kv.Value); break;
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
