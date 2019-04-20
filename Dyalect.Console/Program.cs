using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dyalect
{
    public static class Program
    {
        class Dya { }

        private const int ERR = -1;
        private const int OK = 0;
        private static DyaOptions options;
        private static IDictionary<string, object> config;
        private static string startupPath;

        public static int Main(string[] args)
        {
            startupPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

            if (!Prepare(args))
                return ERR;

            Printer.NoColors = options.NoColors;

            var buildOptions = new BuilderOptions
            {
                Debug = options.Debug,
                NoLangModule = options.NoLang
            };

            var lookup = FileLookup.Create(startupPath, options.Paths);
            var linker = new DyIncrementalLinker(lookup, buildOptions);

            Printer.Clear();
            Console.Title = $"Dyalect - {startupPath}";

            if (!options.NoLogo)
            {
                Printer.Header($"Dya (Dyalect Interactive Console). Built {File.GetLastWriteTime(GetPathByType<Dya>())}");
                Printer.Subheader($"Dya version {Meta.Version}");
                Printer.Subheader($"Running {Environment.OSVersion}");
            }

            if (options.FileName != null)
            {
                Printer.LineFeed();
                return RunAndBye(linker) ? OK : ERR;
            }
            else
            {
                DyMachine dym = null;

                while (true)
                {
                    Printer.LineFeed();
                    Printer.Prefix("dy>");

                    var line = Console.ReadLine();
                    var cm = TryRunCommand(line);

                    if (cm != null)
                    {
                        if (cm.Value == CommandResult.Reset)
                        {
                            linker = new DyIncrementalLinker(lookup, buildOptions);
                            dym = null;
                            Printer.Output("Virtual machine is reseted.");
                        }
                        else if (cm.Value == CommandResult.Exit)
                            break;

                        continue;
                    }

                    var made = linker.Make(SourceBuffer.FromString(line));

                    if (!made.Success)
                    {
                        Printer.PrintErrors(made.Messages);
                        continue;
                    }

                    if (dym == null)
                        dym = new DyMachine(made.Value);

                    Execute(dym);
                }
            }

            return OK;
        }

        private static CommandResult? TryRunCommand(string cmd)
        {
            if (cmd.Length > 1 && cmd[0] == CommandDispatcher.Prefix[0])
            {
                var command = cmd.Substring(1).Trim();
                int idx;
                object argument = null;

                if ((idx = command.IndexOf(' ')) != -1)
                {
                    command = command.Substring(0, idx);
                    argument = command.Substring(idx + 1, command.Length - idx - 1);
                }

                return CommandDispatcher.Dispatch(command, argument);
            }
            else
                return null;
        }

        private static bool Prepare(string[] args)
        {
            try
            {
                config = ConfigReader.Read(Path.Combine(startupPath, "config.json"));
                options = CommandLineReader.Read<DyaOptions>(args, config);
            }
            catch (DyaException ex)
            {
                Printer.Error(ex.Message);
                return false;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(options.Theme))
                {
                    var fullPath = Path.Combine(startupPath, "themes", options.Theme + ".json");

                    if (!File.Exists(fullPath))
                    {
                        Printer.Error($"Unable to find theme file: {fullPath}.");
                        return false;
                    }

                    var colors = ConfigReader.Read(fullPath);
                    Theme.Read(colors);
                }
                else
                    Theme.SetDefault();
            }
            catch (DyaException ex)
            {
                Theme.SetDefault();
                Printer.Error(ex.Message);
                return false;
            }

            return true;
        }

        private static bool RunAndBye(DyLinker linker)
        {
            var made = linker.Make(SourceBuffer.FromFile(options.FileName));

            if (!made.Success)
            {
                Printer.PrintErrors(made.Messages);
                return false;
            }

            var dym = new DyMachine(made.Value);
            return Execute(dym);
        }

        private static string GetPathByType<T>()
        {
            var codeBase = typeof(T).Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        private static bool Execute(DyMachine dym)
        {
            try
            {
                var res = dym.Execute();
                Printer.Output(res);
                return true;
            }
            catch (DyCodeException ex)
            {
                Printer.Error(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                Printer.Error(ex.Message);
                return false;
            }
        }
    }
}
