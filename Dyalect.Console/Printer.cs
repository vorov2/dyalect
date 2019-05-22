using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using Dyalect.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Console;

namespace Dyalect
{
    internal static class Printer
    {
        public static bool NoLogo { get; set; }

        public static void LineFeed() => WriteLine();

        public static void Prefix(string data) => Write(data);

        public static void Error(string data) => WriteLine(data);

        public static void Warning(string data) => WriteLine(data);

        public static void Information(string data) => WriteLine(data);

        public static void Output(string data) => WriteLine(data);

        public static void Output(ExecutionResult res)
        {
            if (res.Reason != TerminationReason.Complete)
            {
                Error($"Terminated, reason: {res.Reason}");
                return;
            }

            if (res.Value == DyNil.Instance)
                return;

            var fmt = Format(res.Value, res.Context);
            Output(fmt);
        }

        public static string Format(DyObject obj, ExecutionContext ctx, bool notype = false)
        {
            string fmt;

            try
            {
                fmt = obj.Format(ctx);
            }
            catch (Exception ex)
            {
                return $"[Error evaluating result value: {ex.Message}]";
            }

            return notype ? fmt : fmt + " :: " + obj.TypeName(ctx);
        }

        public static void SupplementaryOutput(string data) => WriteLine(data);

        public static void Header()
        {
            if (!NoLogo)
            {
                Title = $"Dyalect - {FS.GetStartupPath()}";
                Header($"Dya (Dyalect Interactive Console). Built {FS.GetAssembyTimeStamp()}");
                Subheader($"Dya version {Meta.Version}");
                Subheader($"Running {Environment.OSVersion}");
            }
        }

        private static void Header(params string[] lines)
        {
            foreach (var l in lines)
                WriteLine(l);
        }

        private static void Subheader(string data) => WriteLine(data);

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
    }
}
