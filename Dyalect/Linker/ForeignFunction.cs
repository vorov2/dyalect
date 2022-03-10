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
            dynamic val(int i)
            {
                try
                {
                    return TypeConverter.ConvertTo(ctx, args[i], fun.Types[i])!;
                }
                catch (Exception)
                {
                    return ctx.InvalidType(args[i].GetTypeInfo(ctx).TypeName);
                }
            }

            object? retval = (fun.Types!.Length - 1) switch
            {
                1 => fun.Invoke(ctx, expectContext, fun.Types!.Length),
                2 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0)),
                3 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1)),
                4 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2)),
                5 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3)),
                6 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4)),
                7 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5)),
                8 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6)),
                9 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7)),
                10 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8)),
                11 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9)),
                12 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10)),
                13 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11)),
                14 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12)),
                15 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13)),
                16 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13), val(14)),
                17 => fun.Invoke(ctx, expectContext, fun.Types!.Length, val(0), val(1), val(2), val(3), val(4), val(5), val(6), val(7), val(8), val(9), val(10), val(11), val(12), val(13), val(14), val(15)),
                _ => throw new NotSupportedException()
            };

            if (ctx.HasErrors)
                return DyNil.Instance;

            try
            {
                return TypeConverter.ConvertFrom(retval, fun.Types[^1]);
            }
            catch (Exception)
            {
                return ctx.InvalidType(fun.Types[^1].Name);
            }
        }

        internal override bool Equals(DyFunction func) => func is ForeignFunction m && m.fun.Equals(fun);

        public override int GetHashCode() => fun.Func!.GetHasCode();

        protected override DyFunction Clone(ExecutionContext ctx) => this;
    }
}
