using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Linker
{
    internal sealed class ForeignFunction : DyForeignFunction
    {
        private readonly FunctionDescriptor fun;

        public ForeignFunction(string name, FunctionDescriptor fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
        {
            this.fun = fun;
        }

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            return fun.ArgumentCount switch
            {
                0  => fun.Func(),
                1  => fun.Func((dynamic)args[0]),
                2  => fun.Func((dynamic)args[0], (dynamic)args[1]),
                3  => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2]),
                4  => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3]),
                5  => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4]),
                6  => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5]),
                7  => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6]),
                8  => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7]),
                9  => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7], (dynamic)args[8]),
                10 => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7], (dynamic)args[8], (dynamic)args[9]),
                11 => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7], (dynamic)args[8], (dynamic)args[9], (dynamic)args[10]),
                12 => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7], (dynamic)args[8], (dynamic)args[9], (dynamic)args[10], (dynamic)args[11]),
                13 => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7], (dynamic)args[8], (dynamic)args[9], (dynamic)args[10], (dynamic)args[11], (dynamic)args[12]),
                14 => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7], (dynamic)args[8], (dynamic)args[9], (dynamic)args[10], (dynamic)args[11], (dynamic)args[12], (dynamic)args[13]),
                15 => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7], (dynamic)args[8], (dynamic)args[9], (dynamic)args[10], (dynamic)args[11], (dynamic)args[12], (dynamic)args[13], (dynamic)args[14]),
                16 => fun.Func((dynamic)args[0], (dynamic)args[1], (dynamic)args[2], (dynamic)args[3], (dynamic)args[4], (dynamic)args[5], (dynamic)args[6], (dynamic)args[7], (dynamic)args[8], (dynamic)args[9], (dynamic)args[10], (dynamic)args[11], (dynamic)args[12], (dynamic)args[13], (dynamic)args[14], (dynamic)args[15]),
                _  => throw new NotSupportedException()
            };
        }

        internal override bool Equals(DyFunction func) => func is ForeignFunction m && m.fun.Equals(fun);

        protected override DyFunction Clone(ExecutionContext ctx) => this;
    }
}
