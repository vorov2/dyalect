using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dyalect.Util
{
    public static class CommandDispatcher
    {
        public const string Prefix = ":";

        private static Dictionary<string, Func<object, CommandResult>> commands;

        public static CommandResult Dispatch(string command, object argument)
        {
            if (commands == null)
            {
                commands = new Dictionary<string, Func<object, CommandResult>>(StringComparer.OrdinalIgnoreCase);
                var mis = typeof(CommandDispatcher).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

                foreach (var m in mis)
                {
                    var attr = Attribute.GetCustomAttribute(m, typeof(BindingAttribute)) as BindingAttribute;

                    if (attr == null)
                        continue;

                    var act = (Func<object, CommandResult>)m.CreateDelegate(typeof(Func<object, CommandResult>));

                    foreach (var n in attr.Names)
                        commands.Add(n, act);
                }
            }

            if (!commands.TryGetValue(command, out var cmd))
            {
                Printer.Error($"Unknown command .{command}.");
                return CommandResult.None;
            }

            return cmd(argument);
        }

        [Binding("bye", "exit", Help = "Exits console.")]
        private static CommandResult Exit(object arg)
        {
            Printer.Output("Bye!");
            return CommandResult.Exit;
        }

        [Binding("cls", "clear", Help = "Clears the console window.")]
        private static CommandResult Clear(object arg)
        {
            Console.Clear();
            return CommandResult.None;
        }

        [Binding("reset", Help = "Resets the interactive session.")]
        private static CommandResult Reset(object arg)
        {
            return CommandResult.Reset;
        }

        [Binding("help", Help = "Displays this help screen")]
        private static CommandResult Help(object arg)
        {
            var switches = HelpGenerator.Generate<DyaOptions>("-", 12).TrimEnd('\r', '\n');
            var commands = HelpGenerator.Generate(typeof(CommandDispatcher), Prefix, 12).TrimEnd('\r', '\n');

            Printer.Output("Command line switches:");
            Printer.Output(switches);
            Printer.Output("Commands:");
            Printer.Output(commands);

            return CommandResult.None;
        }
    }
}
