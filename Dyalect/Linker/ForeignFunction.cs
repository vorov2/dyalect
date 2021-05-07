using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Linker
{
    internal sealed class ForeignFunction : DyForeignFunction
    {
        private readonly FunctionDescriptor fun;
        private readonly bool expectContext;

        public ForeignFunction(string name, FunctionDescriptor fun, Par[] pars, int varArgIndex, bool expectContext) : base(name, pars, varArgIndex)
        {
            this.fun = fun;
            this.expectContext = expectContext;
        }

        internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args)
        {
            dynamic val(int i) => TypeConverter.ConvertTo(args[i], fun.Types[i])!;
            object retval;

            if (expectContext)
            {
                retval = (fun.Types!.Length - 1) switch
                {
                    0  => fun.Func(ctx),
                    1  => fun.Func(ctx, val(0)),
                    2  => fun.Func(ctx, val(0), val(1)),
                    3  => fun.Func(ctx, val(0), val(1), val(2)),
                    4  => fun.Func(ctx, val(0), val(1), val(2), val(3)),
                    5  => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4)),
                    6  => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5)),
                    7  => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6)),
                    8  => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7)),
                    9  => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8)),
                    10 => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9)),
                    11 => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10)),
                    12 => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11)),
                    13 => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12)),
                    14 => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13)),
                    15 => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13), val(14)),
                    16 => fun.Func(ctx, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13), val(14), val(15)),
                    _  => throw new NotSupportedException()
                };
            }
            else
            {
                retval = (fun.Types!.Length - 1) switch
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

            return TypeConverter.ConvertFrom(retval, fun.Types[^1]);
        }

        internal override bool Equals(DyFunction func) => func is ForeignFunction m && m.fun.Equals(fun);

        public override int GetHashCode() => fun.Func!.GetHasCode();

        protected override DyFunction Clone(ExecutionContext ctx) => this;
    }
}
