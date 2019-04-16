using Dyalect.Command;
using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dyalect
{
    public static class Program
    {
        private const int ERR = -1;
        private const int OK = 0;
        private static ProgramOptions options;

        public static int Main(string[] args)
        {
            if (!Prepare(args))
                return ERR;

            var buildOptions = new BuilderOptions
            {
                Debug = options.Debug,
                NoLangModule = options.NoLang
            };

            var lookup = FileLookup.Create(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), options.Paths);
            var linker = new DyIncrementalLinker(lookup, buildOptions);

            Printer.Clear();
            Printer.Header(
                $"Dya (Dyalect Interactive Console). Built {File.GetLastWriteTime(GetPathByType<Option>())}",
                $"Dya version {Meta.Version}"
                );
            Printer.LineFeed();

            if (options.FileName != null)
                return RunAndBye(linker) ? OK : ERR;
            else
            {
                DyMachine dym = null;

                while (true)
                {
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
                        PrintErrors(made.Messages);
                        continue;
                    }

                    if (dym == null)
                        dym = new DyMachine(made.Value);

                    try
                    {
                        var res = dym.Execute();
                        Printer.Output(res.Value.ToString());
                    }
                    catch (DyCodeException ex)
                    {
                        Printer.Error(ex.ToString());
                    }
                    catch (DyRuntimeException ex)
                    {
                        Printer.Error(ex.Message);
                    }
                }
            }

            return OK;
        }

        private static CommandResult? TryRunCommand(string cmd)
        {
            if (cmd.Length > 1 && cmd[0] == '.')
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
                options = CommandLineReader.Read<ProgramOptions>(args);
            }
            catch (CommandException ex)
            {
                Config.SetDefault();
                Printer.Error(ex.Message);
                return false;
            }

            return Initialize(options);
        }

        private static bool RunAndBye(DyLinker linker)
        {
            var made = linker.Make(SourceBuffer.FromFile(options.FileName));

            if (!made.Success)
            {
                PrintErrors(made.Messages);
                return false;
            }

            var dym = new DyMachine(made.Value);
            var res = dym.Execute();
            Printer.Output(res.Value.ToString());
            return true;
        }

        private static void PrintErrors(IEnumerable<BuildMessage> messages)
        {
            foreach (var m in messages)
            {
                if (m.Type == BuildMessageType.Error)
                    Printer.Error(m.ToString());
                else if (m.Type == BuildMessageType.Warning)
                    Printer.Warning(m.ToString());
                else
                    Printer.Information(m.ToString());
            }
        }

        private static void PrintErrors(IEnumerable<JsonParser.Error> messages)
        {
            foreach (var m in messages)
                Printer.Error(m.ToString());
        }

        private static string GetPathByType<T>()
        {
            var codeBase = typeof(T).Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        private static bool Initialize(ProgramOptions opts)
        {
            const string FILENAME = "config.json";
            var path = Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), FILENAME);

            if (!File.Exists(path))
            {
                Config.SetDefault();
                Printer.Error($"Config file \"{FILENAME}\" not found.");
                return false;
            }

            try
            {
                var json = new JsonParser(File.ReadAllText(path));
                var dict = json.Parse() as IDictionary<string, object>;

                if (!json.Success)
                {
                    PrintErrors(json.Errors);
                    return false;
                }

                if (dict == null)
                {
                    Printer.Error("Invalid configuration file format.");
                    return false;
                }

                if (!dict.TryGetValue("colors", out var colorObj) || !(colorObj is IDictionary<string, object> colors))
                {
                    Printer.Error("Missing console color information from configuration file.");
                    return false;
                }

                Config.Read(colors);
                return true;
            }
            catch (Exception ex)
            {
                Config.SetDefault();
                Printer.Error($"Error reading configuration file: {ex.Message}");
                return false;
            }
        }
    }
}
