using Dyalect.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dyalect.Util
{
    public sealed class CommandDispatcher
    {
        public const string Prefix = "#";

        private Dictionary<string, CommandCallBack> commands = null!;

        private readonly InteractiveContext ctx;

        internal delegate void CommandCallBack(object? arg);

        internal CommandDispatcher(InteractiveContext ctx)
        {
            this.ctx = ctx;
        }

        public void Dispatch(string command, object? argument)
        {
            if (commands is null)
            {
                commands = new Dictionary<string, CommandCallBack>(StringComparer.OrdinalIgnoreCase);
                var mis = typeof(CommandDispatcher).GetMethods();

                foreach (var m in mis)
                {
                    if (Attribute.GetCustomAttribute(m, typeof(BindingAttribute)) is not BindingAttribute attr)
                        continue;

                    var act = (CommandCallBack)m.CreateDelegate(typeof(CommandCallBack), this);

                    foreach (var n in attr.Names)
                        commands.Add(n, act);
                }
            }

            if (!commands.TryGetValue(command, out var cmd))
            {
                Printer.Error($"Unknown command #{command}.");
                return;
            }

            cmd(argument);
        }

        [Binding("bye", "exit", Help = "Exits console.")]
        public void Exit(object _)
        {
            Printer.Output("Bye!");
            Environment.Exit(0);
        }

        [Binding("cls", "clear", Help = "Clears the console window.")]
        public void Clear(object _)
        {
            Console.Clear();
        }

        [Binding("reset", Help = "Resets the interactive session.")]
        public void Reset(object _)
        {
            ctx.Reset();
            Printer.Output("Virtual machine is reseted.");
        }

        [Binding("help", Help = "Displays this help screen.")]
        public void Help(object _)
        {
            var switches = HelpGenerator.Generate<DyaOptions>("-").TrimEnd('\r', '\n');
            var commands = HelpGenerator.Generate(typeof(CommandDispatcher), Prefix).TrimEnd('\r', '\n');

            Printer.LineFeed();
            Printer.Output("Command line switches:");
            Printer.Output(switches);
            Printer.LineFeed();
            Printer.Output("Commands:");
            Printer.Output(commands);
        }

        [Binding("dir", Help = "Shows current working directory")]
        public void Directory(object _)
        {
            Printer.Output(Environment.CurrentDirectory);
        }

        [Binding("options", Help = "Displays current console options.")]
        public void ShowOptions(object _)
        {
            Printer.LineFeed();
            Printer.Output("Current options:");
            Printer.Output(ctx.Options.ToString());
        }

        [Binding("dump", Help = "Dumps global variables and prints their values.")]
        public void Dump(object _)
        {
            Printer.LineFeed();
            Printer.Output("Dump of globals:");

            if (ctx.ExecutionContext == null)
            {
                Printer.Output("...none");
                return;
            }

            var xs = DyMachine.DumpVariables(ctx.ExecutionContext).ToList();
            var vals = new string[xs.Count];
            var types = new string[xs.Count];
            var (keyLen, valLen) = (0, 0);

            for (var i = 0; i < xs.Count; i++)
            {
                var rv = xs[i];
                vals[i] = Printer.Format(rv.Value, ctx.ExecutionContext, notype: true, maxLen: 32);
                types[i] = rv.Value.GetTypeInfo(ctx.ExecutionContext).ReflectedTypeName;

                if (keyLen < rv.Name.Length) keyLen = rv.Name.Length;
                if (valLen < vals[i].Length) valLen = vals[i].Length;
            }

            for (var i = 0; i < xs.Count; i++)
            {
                var rv = xs[i];
                Printer.Output($"{rv.Name}{new string(' ', keyLen - rv.Name.Length)} | {vals[i]}{new string(' ', valLen - vals[i].Length)} | {types[i]}");
            }
        }

        [Binding("eval", Help = "Evaluates a given file in a current interactive session.")]
        public void Eval(object arg)
        {
            var str = arg?.ToString()?.Trim('\"', '\'');

            if (str is not null && ctx.EvalFile(str, measureTime: false))
                Printer.Output($"File \"{Path.GetFileName(str)}\" successfully evaluated.");
        }
    }
}
