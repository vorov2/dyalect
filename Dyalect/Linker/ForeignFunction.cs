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
            object val(int i) => args[i].ChangeType(fun.Types[i]);

            return fun.Types.Length - 1 switch
            {
                0  => fun.Func(),
                1  => fun.Func(val(0)),
                2  => fun.Func(val(0), val(1)),
                3  => fun.Func(val(0), val(1), val(2)),
                4  => fun.Func(val(0), val(1), val(2), val(3)),
                5  => fun.Func(val(0), val(1), val(2), val(3), val(4)),
                6  => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5)),
                7  => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6)),
                8  => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7)),
                9  => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8)),
                10 => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9)),
                11 => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10)),
                12 => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11)),
                13 => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12)),
                14 => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13)),
                15 => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13), val(14)),
                16 => fun.Func(val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13), val(14), val(15)),
                _  => throw new NotSupportedException()
            };
        }

        internal override bool Equals(DyFunction func) => func is ForeignFunction m && m.fun.Equals(fun);

        protected override DyFunction Clone(ExecutionContext ctx) => this;
    }
}
