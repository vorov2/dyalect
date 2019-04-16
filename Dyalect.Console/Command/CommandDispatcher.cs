using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dyalect.Command
{
    public static class CommandDispatcher
    {
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

        [Binding("bye", "exit")]
        private static CommandResult Exit(object arg)
        {
            Printer.Output("Bye!");
            return CommandResult.Exit;
        }

        [Binding("cls", "clear")]
        private static CommandResult Clear(object arg)
        {
            Console.Clear();
            return CommandResult.None;
        }

        [Binding("reset")]
        private static CommandResult Reset(object arg)
        {
            return CommandResult.Reset;
        }
    }
}
