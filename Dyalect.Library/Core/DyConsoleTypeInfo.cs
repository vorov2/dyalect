using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyConsoleTypeInfo : DyForeignTypeInfo
    {
        public override string TypeName => "Console";

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
                Console.WriteLine(str.GetString());

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
            return new DyTuple(new("left", left), new("top", top));
        }

        private DyObject SetCursorPosition(ExecutionContext ctx, DyObject left, DyObject top)
        {
            if (left.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, left);

            if (top.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, top);

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
            if (color.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, color);

            if (!Enum.TryParse<ConsoleColor>(color.ToString(), true, out var res))
                return ctx.InvalidValue(color);

            Console.BackgroundColor = res;
            return DyNil.Instance;
        }

        private DyObject GetForeColor(ExecutionContext _) => new DyString(Console.ForegroundColor.ToString());

        private DyObject SetForeColor(ExecutionContext ctx, DyObject color)
        {
            if (color.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, color);

            if (!Enum.TryParse<ConsoleColor>(color.ToString(), true, out var res))
                return ctx.InvalidValue(color);

            Console.ForegroundColor = res;
            return DyNil.Instance;
        }

        private DyObject ResetColor(ExecutionContext _)
        {
            Console.ResetColor();
            return DyNil.Instance;
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Write" => Func.Static(name, Write, -1, new Par("value")),
                "WriteLine" => Func.Static(name, WriteLine, -1, new Par("value", DyString.Empty)),
                "Read" => Func.Static(name, Read),
                "ReadLine" => Func.Static(name, ReadLine),
                "Clear" => Func.Static(name, Clear),
                "ResetColor" => Func.Static(name, ResetColor),
                "GetCursorPosition" => Func.Static(name, GetCursorPosition),
                "SetCursorPosition" => Func.Static(name, SetCursorPosition, -1, new Par("left"), new Par("right")),
                "BackColor" => Func.Auto(name, GetBackColor),
                "__set_BackColor" => Func.Static(name, SetBackColor, -1, new Par("value")),
                "ForeColor" => Func.Auto(name, GetForeColor),
                "__set_ForeColor" => Func.Static(name, SetForeColor, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
