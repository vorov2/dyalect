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

        private ConsoleColor GetColor(DyObject obj)
        {
            if (Enum.TryParse<ConsoleColor>(obj.GetString(), true, out var value))
                return value;

            return ConsoleColor.Black;
        }

        private DyObject WriteToConsole(ExecutionContext ctx, DyObject value, DyObject color, DyObject backColor, bool newLine)
        {
            var str = value.ToString(ctx);

            if (ctx.HasErrors)
                return Default();

            if (color.NotNil() && !color.IsString(ctx)) return Default();
            if (backColor.NotNil() && !backColor.IsString(ctx)) return Default();

            var oldColor = Console.ForegroundColor;
            var oldBackColor = Console.BackgroundColor;

            if (color.NotNil()) Console.ForegroundColor = GetColor(color);
            if (backColor.NotNil()) Console.BackgroundColor = GetColor(backColor);

            if (newLine)
                Console.Write(str.GetString() + Environment.NewLine);
            else
                Console.Write(str.GetString());

            Console.ForegroundColor = oldColor;
            Console.BackgroundColor = oldBackColor;
            return DyNil.Instance;
        }

        private DyObject WriteLine(ExecutionContext ctx, DyObject value, DyObject color, DyObject backColor) =>
            WriteToConsole(ctx, value, color, backColor, newLine: true);

        private DyObject Write(ExecutionContext ctx, DyObject value, DyObject color, DyObject backColor) =>
           WriteToConsole(ctx, value, color, backColor, newLine: false);

        private DyObject Read(ExecutionContext _) => new DyChar((char)Console.Read());

        private DyObject ReadLine(ExecutionContext _) => new DyString(Console.ReadLine() ?? "");

        private ConsoleColor? defaultBackColor;
        private DyObject Clear(ExecutionContext ctx, DyObject backColor)
        {
            if (defaultBackColor is null)
                defaultBackColor = Console.BackgroundColor;
            
            if (backColor.NotNil() && !backColor.IsString(ctx)) return Default();
            Console.BackgroundColor = backColor.NotNil() ? GetColor(backColor) : defaultBackColor.Value;
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

        private DyObject SetTitle(ExecutionContext ctx, DyObject value)
        {
            if (!value.IsString(ctx)) return Default();
            Console.Title = value.GetString();
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
                "Write" => Func.Static(name, Write, -1, new Par("value"), new Par("color", DyNil.Instance), new Par("backColor", DyNil.Instance)),
                "WriteLine" => Func.Static(name, WriteLine, -1, new Par("value", DyString.Empty), new Par("color", DyNil.Instance), new Par("backColor", DyNil.Instance)),
                "Read" => Func.Static(name, Read),
                "ReadLine" => Func.Static(name, ReadLine),
                "ReadKey" => Func.Static(name, ReadKey, -1, new Par("intercept", DyBool.False)),
                "Clear" => Func.Static(name, Clear, -1, new Par("backColor", DyNil.Instance)),
                "GetCursorPosition" => Func.Static(name, GetCursorPosition),
                "SetCursorPosition" => Func.Static(name, SetCursorPosition, -1, new Par("left"), new Par("right")),
                "SetTitle" => Func.Static(name, SetTitle, -1, new Par("value")),
                "SetOutput" => Func.Static(name, SetOutput, -1, new Par("write", DyNil.Instance), new Par("writeLine", DyNil.Instance)),
                "SetInput" => Func.Static(name, SetInput, -1, new Par("read", DyNil.Instance), new Par("readLine", DyNil.Instance)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
