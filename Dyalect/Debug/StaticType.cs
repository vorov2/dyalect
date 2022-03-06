using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Debug
{
    public abstract class StaticType
    {
        protected StaticType() { }

        public abstract DyObject ToRuntimeType(RuntimeContext ctx);
    }

    internal sealed class StaticInteger : StaticType
    {
        public static readonly StaticInteger Zero = new(0);

        private readonly long value;

        public StaticInteger(long value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => ctx.Integer.Get(value);

        public override string ToString() => value.ToString(CI.NumberFormat);
    }

    internal sealed class StaticFloat : StaticType
    {
        private readonly double value;

        public StaticFloat(double value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => new DyFloat(ctx.Float, value);

        public override string ToString() => value.ToString(CI.NumberFormat);
    }

    internal sealed class StaticChar : StaticType
    {
        public static readonly StaticChar WhiteSpace = new(' ');

        private readonly char value;

        public StaticChar(char value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => new DyChar(ctx.Char, value);

        public override string ToString() => value.ToString();
    }

    internal sealed class StaticString : StaticType
    {
        private readonly string value;

        public StaticString(string value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => new DyString(ctx.String, ctx.Char, value);

        public override string ToString() => value;
    }

    internal sealed class StaticBool : StaticType
    {
        public static readonly StaticBool True = new(true);
        public static readonly StaticBool False = new(false);

        private readonly bool value;

        public StaticBool(bool value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => value ? ctx.Bool.True : ctx.Bool.False;

        public override string ToString() => value ? "true" : "false";
    }

    internal sealed class StaticNil : StaticType
    {
        public static readonly StaticNil Instance = new();

        private StaticNil() { }

        public override DyObject ToRuntimeType(RuntimeContext ctx) => ctx.Nil.Instance;

        public override string ToString() => "nil";
    }
}
