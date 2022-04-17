using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.IO;

namespace Dyalect.Library.Core
{
    public sealed class DyConsoleTypeInfo : DyForeignTypeInfo
    {
        public override string TypeName => "Console";
        private TextWriter? consoleOutput;
        private TextReader? consoleInput;

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private DyObject Write(ExecutionContext ctx, DyObject value)
        {
            var str = value.ToString(ctx);

            if (!ctx.HasErrors)
                Console.Write(str.GetString());

            return DyNil.Instance;
        }

        private DyObject WriteLine(ExecutionContext ctx, DyObject value)
        {
            var str = value.ToString(ctx);

            if (!ctx.HasErrors)
                Console.Write(str.GetString() + Environment.NewLine);

            return DyNil.Instance;
        }

        private DyObject Read(ExecutionContext _) => new DyChar((char)Console.Read());

        private DyObject ReadLine(ExecutionContext _) => new DyString(Console.ReadLine() ?? "");

        private DyObject Clear(ExecutionContext _)
        {
            Console.Clear();
            return DyNil.Instance;
        }

        private DyObject GetCursorPosition(ExecutionContext _)
        {
            var (left, top) = Console.GetCursorPosition();
            return DyTuple.Create(new("left", left), new("top", top));
        }

        private DyObject SetCursorPosition(ExecutionContext ctx, DyObject left, DyObject top)
        {
            if (!left.IsInteger(ctx)) return Default();
            if (!top.IsInteger(ctx)) return Default();

            try
            {
                Console.SetCursorPosition((int)left.GetInteger(), (int)top.GetInteger());
                return DyNil.Instance;
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue();
            }
        }

        private DyObject GetBackColor(ExecutionContext _) => new DyString(Console.BackgroundColor.ToString());

        private DyObject SetBackColor(ExecutionContext ctx, DyObject color)
        {
            if (!color.IsString(ctx)) return Default();

            if (!Enum.TryParse<ConsoleColor>(color.ToString(), true, out var res))
                return ctx.InvalidValue(color);

            Console.BackgroundColor = res;
            return DyNil.Instance;
        }

        private DyObject GetForeColor(ExecutionContext _) => new DyString(Console.ForegroundColor.ToString());

        private DyObject SetForeColor(ExecutionContext ctx, DyObject color)
        {
            if (!color.IsString(ctx)) return Default();

            if (!Enum.TryParse<ConsoleColor>(color.ToString(), true, out var res))
                return ctx.InvalidValue(color);

            Console.ForegroundColor = res;
            return DyNil.Instance;
        }

        private DyObject SetTitle(ExecutionContext ctx, DyObject value)
        {
            if (!value.IsString(ctx)) return Default();
            Console.Title = value.GetString();
            return DyNil.Instance;
        }

        private DyObject ResetColor(ExecutionContext _)
        {
            Console.ResetColor();
            return DyNil.Instance;
        }

        private DyObject SetOutput(ExecutionContext ctx, DyObject write, DyObject writeLine)
        {
            if (write is DyNil && writeLine is DyNil)
            {
                if (consoleOutput is not null)
                    Console.SetOut(consoleOutput);
            }
            else if (write is not DyFunction writeFn)
                ctx.InvalidType(write);
            else if (writeLine is not DyFunction writeLineFn)
                ctx.InvalidType(writeLine);
            else
            {
                if (consoleOutput is null)
                    consoleOutput = Console.Out;

                Console.SetOut(new ConsoleTextWriter(ctx, writeFn, writeLineFn));
            }

            return DyNil.Instance;
        }

        private DyObject SetInput(ExecutionContext ctx, DyObject read, DyObject readLine)
        {
            if (read is DyNil && readLine is DyNil)
            {
                if (consoleInput is not null)
                    Console.SetIn(consoleInput);
            }
            else if (read is not DyFunction readFn)
                ctx.InvalidType(read);
            else if (readLine is not DyFunction readLineFn)
                ctx.InvalidType(readLine);
            else
            {
                if (consoleInput is null)
                    consoleInput = Console.In;

                Console.SetIn(new ConsoleTextReader(ctx, readFn, readLineFn));
            }

            return DyNil.Instance;
        }

        private DyObject ReadKey(ExecutionContext ctx, DyObject intercept)
        {
            try
            {
                var ci = Console.ReadKey(intercept.IsTrue());
                return DyTuple.Create(
                    new("key", ci.Key.ToString()),
                    new("keyChar", ci.KeyChar),
                    new("alt", (ci.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt),
                    new("shift", (ci.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift),
                    new("ctrl", (ci.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                );
            }
            catch (InvalidOperationException)
            {
                return ctx.InvalidOperation();
            }
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Write" => Func.Static(name, Write, -1, new Par("value")),
                "WriteLine" => Func.Static(name, WriteLine, -1, new Par("value", DyString.Empty)),
                "Read" => Func.Static(name, Read),
                "ReadLine" => Func.Static(name, ReadLine),
                "ReadKey" => Func.Static(name, ReadKey, -1, new Par("intercept", DyBool.False)),
                "Clear" => Func.Static(name, Clear),
                "ResetColor" => Func.Static(name, ResetColor),
                "GetCursorPosition" => Func.Static(name, GetCursorPosition),
                "SetCursorPosition" => Func.Static(name, SetCursorPosition, -1, new Par("left"), new Par("right")),
                "BackColor" => Func.Auto(name, GetBackColor),
                "set_BackColor" => Func.Static(name, SetBackColor, -1, new Par("value")),
                "ForeColor" => Func.Auto(name, GetForeColor),
                "set_ForeColor" => Func.Static(name, SetForeColor, -1, new Par("value")),
                "SetTitle" => Func.Static(name, SetTitle, -1, new Par("value")),
                "SetOutput" => Func.Static(name, SetOutput, -1, new Par("write", DyNil.Instance), new Par("writeLine", DyNil.Instance)),
                "SetInput" => Func.Static(name, SetInput, -1, new Par("read", DyNil.Instance), new Par("readLine", DyNil.Instance)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
