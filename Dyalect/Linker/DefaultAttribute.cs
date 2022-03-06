using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Linker
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class DefaultAttribute : Attribute
    {
        public DefaultAttribute(string value) =>
            Value = new DyString(value);

        public DefaultAttribute(long value) =>
            Value = ctx.RuntimeContext.Integer.Get(value);

        public DefaultAttribute(double value) =>
            Value = new DyFloat(value);

        public DefaultAttribute(char value) =>
            Value = new DyChar(value);

        public DefaultAttribute() =>
            Value = ctx.RuntimeContext.Nil.Instance;

        public DefaultAttribute(bool value) =>
            Value = value ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

        public DyObject Value { get; }
    }
}
