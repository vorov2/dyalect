using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using Dyalect.Util;
using System;
using System.Collections.Generic;

namespace Dyalect
{
    internal static class Printer
    {
        public static bool NoColors { get; set; }

        public static void Clear()
        {
            Console.BackgroundColor = Theme.Background;
            Console.ForegroundColor = Theme.Foreground;
            Console.Clear();
        }

        public static void LineFeed() => Console.WriteLine();

        public static void Prefix(string data) => WithColor(Theme.Prefix, Write(data));

        public static void Error(string data) => WithColor(Theme.Error, WriteLine(data));

        public static void Warning(string data) => WithColor(Theme.Warning, WriteLine(data));

        public static void Information(string data) => WithColor(Theme.Info, WriteLine(data));

        public static void Output(string data) => WithColor(Theme.Output, WriteLine(data));

        public static void Output(ExecutionResult res)
        {
            if (res.Reason != TerminationReason.Complete)
            {
                Error($"Terminated, reason: {res.Reason}");
                return;
            }

            var fmt = res.Value.Format(res.Context);

            if (res.Context.HasErrors)
                fmt = res.Value.ToString();

            fmt += " :: " + res.Value.TypeName(res.Context);
            Output(fmt);
        }

        public static void SupplementaryOutput(string data) => WithColor(Theme.SupplementaryOutput, WriteLine(data));

        public static void Header(params string[] lines)
        {
            foreach (var l in lines)
                WithColor(Theme.Header, WriteLine(l));
        }

        public static void Subheader(string data) => WithColor(Theme.Subheader, WriteLine(data));

        public static void PrintErrors(IEnumerable<BuildMessage> messages)
        {
            foreach (var m in messages)
            {
                if (m.Type == BuildMessageType.Error)
                    Error(m.ToString());
                else if (m.Type == BuildMessageType.Warning)
                    Warning(m.ToString());
                else
                    Information(m.ToString());
            }
        }

        public static void PrintErrors(IEnumerable<JsonParser.Error> messages)
        {
            foreach (var m in messages)
                Error(m.ToString());
        }

        private static void WithColor(ConsoleColor col, Action act)
        {
            var curcol = Console.ForegroundColor;

            if (!NoColors)
                Console.ForegroundColor = col;

            act();

            if (!NoColors)
                Console.ForegroundColor = curcol;
        }

        private static Action Write(string data) => () => Console.Write(data);

        private static Action WriteLine(string data) => () => Console.WriteLine(data);
    }
}
