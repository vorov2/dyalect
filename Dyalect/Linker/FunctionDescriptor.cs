using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Linker
{
    internal sealed class FunctionDescriptor
    {
        public dynamic? Func;

        public System.Type[]? Types;

        public dynamic? Invoke(ExecutionContext ctx, bool expectedContext, int args, object? p0 = null, object? p1 = null, object? p2 = null, object? p3 = null, object? p4 = null, object? p5 = null, object? p6 = null, object? p7 = null, object? p8 = null, object? p9 = null, object? p10 = null, object? p11 = null, object? p12 = null, object? p13 = null, object? p14 = null, object? p15 = null)
        {
            if (ctx.HasErrors)
                return DyNil.Instance;

            if (expectedContext)
            {
                return args switch
                {
                    1 => Func(ctx),
                    2 => Func(ctx, p0),
                    3 => Func(ctx, p0, p1),
                    4 => Func(ctx, p0, p1, p2),
                    5 => Func(ctx, p0, p1, p2, p3),
                    6 => Func(ctx, p0, p1, p2, p3, p4),
                    7 => Func(ctx, p0, p1, p2, p3, p4, p5),
                    8 => Func(ctx, p0, p1, p2, p3, p4, p5, p6),
                    9 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7),
                    10 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7, p8),
                    11 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9),
                    12 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10),
                    13 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11),
                    14 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12),
                    15 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13),
                    16 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14),
                    17 => Func(ctx, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15),
                    _ => throw new NotSupportedException()
                };
            }
            else
            {
                return args switch
                {
                    1 => Func(),
                    2 => Func(p0),
                    3 => Func(p0, p1),
                    4 => Func(p0, p1, p2),
                    5 => Func(p0, p1, p2, p3),
                    6 => Func(p0, p1, p2, p3, p4),
                    7 => Func(p0, p1, p2, p3, p4, p5),
                    8 => Func(p0, p1, p2, p3, p4, p5, p6),
                    9 => Func(p0, p1, p2, p3, p4, p5, p6, p7),
                    10 => Func(p0, p1, p2, p3, p4, p5, p6, p7, p8),
                    11 => Func(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9),
                    12 => Func(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10),
                    13 => Func(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11),
                    14 => Func(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12),
                    15 => Func(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13),
                    16 => Func(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14),
                    17 => Func(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15),
                    _ => throw new NotSupportedException()
                };
            }
        }
    }
}
