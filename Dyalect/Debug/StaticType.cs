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
        private readonly long value;

        public StaticInteger(long value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => ctx.Integer.Get(value);
    }

    internal sealed class StaticFloat : StaticType
    {
        private readonly double value;

        public StaticFloat(double value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => new DyFloat(ctx.Float, value);
    }

    internal sealed class StaticChar : StaticType
    {
        private readonly char value;

        public StaticChar(char value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => new DyChar(ctx.Char, value);
    }

    internal sealed class StaticString : StaticType
    {
        private readonly string value;

        public StaticString(string value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => new DyString(ctx.String, value);
    }

    internal sealed class StaticBool : StaticType
    {
        private readonly bool value;

        public StaticBool(bool value) => this.value = value;

        public override DyObject ToRuntimeType(RuntimeContext ctx) => value ? ctx.Bool.True : ctx.Bool.False;
    }

    internal sealed class StaticNil : StaticType
    {
        public static readonly StaticNil Instance = new();

        private StaticNil() { }

        public override DyObject ToRuntimeType(RuntimeContext ctx) => ctx.Nil.Instance;
    }
}
