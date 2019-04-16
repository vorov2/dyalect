using System;

namespace Dyalect
{
    internal static class Printer
    {
        public static bool NoColor { get; }

        public static void Clear()
        {
            Console.BackgroundColor = Config.Background;
            Console.ForegroundColor = Config.Foreground;
            Console.Clear();
        }

        public static void LineFeed() => Console.WriteLine();

        public static void Prefix(string data) => WithColor(Config.Prefix, Write(data));

        public static void Error(string data) => WithColor(Config.Error, WriteLine(data));

        public static void Warning(string data) => WithColor(Config.Warning, WriteLine(data));

        public static void Information(string data) => WithColor(Config.Info, WriteLine(data));

        public static void Output(string data) => WithColor(Config.Output, WriteLine(data));

        public static void SupplementaryOutput(string data) => WithColor(Config.SupplementaryOutput, WriteLine(data));

        public static void Header(params string[] lines)
        {
            foreach (var l in lines)
                WithColor(Config.Header, WriteLine(l));
        }

        public static void Subheader(string data) => WithColor(Config.Subheader, WriteLine(data));

        private static void WithColor(ConsoleColor col, Action act)
        {
            var curcol = Console.ForegroundColor;

            if (!NoColor)
                Console.ForegroundColor = col;

            act();

            if (!NoColor)
                Console.ForegroundColor = curcol;
        }

        private static Action Write(string data) => () => Console.Write(data);

        private static Action WriteLine(string data) => () => Console.WriteLine(data);
    }
}
