using Dyalect.Util;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Dyalect
{
    public static class Program
    {
        private const int ERR = -1;
        private const int OK = 0;
        private static CommandDispatcher dispatcher;
        private static InteractiveContext ctx;

        public static int Main(string[] args)
        {
            if (!Prepare(args, out var options))
                return ERR;

            Printer.Header();

            ctx = new InteractiveContext(options);
            dispatcher = new CommandDispatcher(ctx);

            if (options.Test)
            {
                Printer.LineFeed();
                return RunTests(options) ? OK : ERR;
            }
            else if (options.FileNames != null && options.FileNames.Length > 0)
            {
                Printer.LineFeed();

                for (var i = 0; i < options.FileNames.Length; i++)
                {
                    var f = options.FileNames[i];

                    if (i > 0)
                        ctx.Reset();

                    if (!ctx.EvalFile(options.FileNames[0], options.MeasureTime))
                        return ERR;
                }

                if (options.StayInInteractive)
                    RunInteractive();
                else
                    return OK;
            }
            else
                RunInteractive();

            return OK;
        }

        private static bool RunTests(DyaOptions options)
        {
            if (options.FileNames == null || options.FileNames.Length == 0 
                || string.IsNullOrEmpty(options.FileNames[0]))
            {
                Printer.Error("File name(s) not specified.");
                return false;
            }

            return TestRunner.RunTests(options.FileNames, options.AppVeyour);
        }

        private static void RunInteractive()
        {
            var sb = new StringBuilder();
            var balance = 0;

            while (true)
            {
                if (balance == 0)
                    Printer.LineFeed();

                Printer.Prefix(balance == 0 ? "dy>" : "-->");

                var line = Console.ReadLine().Trim();

                if (TryRunCommand(line))
                    continue;

                sb.AppendLine(line);

                balance += line.Count(c => c == '{');
                balance -= line.Count(c => c == '}');

                if (balance > 0)
                    continue;

                ctx.Eval(sb.ToString());
                sb.Clear();
            }
        }

        private static bool TryRunCommand(string cmd)
        {
            if (cmd.Length > 1 && cmd[0] == CommandDispatcher.Prefix[0])
            {
                var command = cmd.Substring(1).Trim();
                int idx;
                object argument = null;

                if ((idx = command.IndexOf(' ')) != -1)
                {
                    var str = command;
                    command = command.Substring(0, idx);
                    argument = str.Substring(idx + 1, str.Length - idx - 1);
                }

                dispatcher.Dispatch(command, argument);
                return true;
            }
            else
                return false;
        }

        private static bool Prepare(string[] args, out DyaOptions options)
        {
            try
            {
                var config = ConfigReader.Read(Path.Combine(FS.GetStartupPath(), "config.json"));
                options = CommandLineReader.Read<DyaOptions>(args, config);
            }
            catch (DyaException ex)
            {
                Printer.Header();
                Printer.LineFeed();
                Printer.Error(ex.Message);
                options = null;
                return false;
            }

            Printer.NoLogo = options.NoLogo;
            return true;
        }
    }
}
