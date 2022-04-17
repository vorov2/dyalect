using Dyalect.Linker;
using Dyalect.UnitTesting;
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
        private static CommandDispatcher dispatcher = null!;
        private static InteractiveContext ctx = null!;

        public static int Main(string[] args)
        {
            if (!Prepare(args, out var options))
                return ERR;

#if !DEBUG
            try
#endif
            {
                return Run(options!) ? OK : ERR;
            }
#if !DEBUG
            catch (Exception ex)
            {
                Printer.Error(ex.Message);
                return ERR;
            }
#endif
        }

        private static bool Run(DyaOptions options)
        {
            Printer.Header();

            ctx = new(options);
            dispatcher = new(ctx);

            if (options.Test)
            {
                Printer.LineFeed();
                return RunTests(options);
            }
            else if (options.Compile)
            {
                Printer.LineFeed();
                return Compile(options);
            }
            else if (options.FileNames != null && options.FileNames.Length > 0)
            {
                Printer.LineFeed();
                var i = 0;

                foreach (var f in options.GetFileNames())
                {
                    if (i > 0)
                        ctx.Reset();

                    if (!ctx.EvalFile(f, options.MeasureTime))
                        return false;

                    i++;
                }

                if (options.StayInInteractive)
                    RunInteractive();
                else
                    return true;
            }
            else
                RunInteractive();

            return true;
        }

        private static bool RunTests(DyaOptions options)
        {
            if (options.FileNames == null || options.FileNames.Length == 0
                || string.IsNullOrEmpty(options.FileNames[0]))
            {
                Printer.Error("File name(s) not specified.");
                return false;
            }

            var tr = new TestRunner(InteractiveContext.CreateBuildOptions(options), options);
            return tr.RunTests(options.GetFileNames());
        }

        private static bool Compile(DyaOptions options)
        {
            var ctx = new InteractiveContext(options);

            foreach (var f in options.GetFileNames())
            {
                var outFile = options.OutputDirectory;

                if (string.IsNullOrWhiteSpace(outFile))
                    outFile = Path.Combine(Path.GetDirectoryName(f)!, Path.GetFileNameWithoutExtension(f) + ".dyo");
                else
                {
                    if (!Directory.Exists(outFile))
                        Directory.CreateDirectory(outFile);

                    outFile = Path.Combine(outFile, Path.GetFileNameWithoutExtension(f) + ".dyo");
                }

                if (!File.Exists(f) || !ctx.Compile(f, out var unit))
                {
                    Printer.Error($"Compilation of file \"{f}\" skipped.");
                    continue;
                }

#if !DEBUG
                try
#endif
                {
                    ObjectFileWriter.Write(outFile, unit);
                    Printer.Information($"Compilation completed. File saved: \"{outFile}\"");
                }
#if !DEBUG
                catch (Exception ex)
                {
                    Printer.Error(ex.Message);
                    Printer.Error($"Compilation of file \"{f}\" skipped.");
                    continue;
                }
#endif
            }

            return true;
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
                var line = Console.ReadLine()?.Trim();

                if (line is null || TryRunCommand(line))
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
                var command = cmd[1..].Trim();
                int idx;
                object? argument = null;

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

        private static bool Prepare(string[] args, out DyaOptions? options)
        {
            try
            {
                var config = ConfigReader.Read(Path.Combine(FileProbe.GetExecutableDirectory(), "config.json"))!;
                options = CommandLineReader.Read<DyaOptions>(args, config);
            }
            catch (Exception ex)
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
