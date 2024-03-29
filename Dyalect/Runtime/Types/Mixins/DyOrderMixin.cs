﻿using Dyalect.Compiler;

namespace Dyalect.Runtime.Types;

internal sealed class DyOrderMixin : DyMixin<DyOrderMixin>
{
    public DyOrderMixin() : base(Dy.Order)
    {
        Members.Add(Builtins.Gt, Binary(Builtins.Gt, Greater));
        Members.Add(Builtins.Lt, Binary(Builtins.Lt, Lesser));
        SetSupportedOperations(Ops.Gt | Ops.Lt | Ops.Gte | Ops.Lte);
    }

    private static DyObject Greater(ExecutionContext ctx, DyObject left, DyObject right)
    {
        try
        {
            return DyTuple.Greater(ctx, ((DyClass)left).Fields, ((DyClass)right).Fields);
        }
        catch(DyCodeException e)
        {
            ctx.Error = e.Error;
            return Nil;
        }
    }

    private static DyObject Lesser(ExecutionContext ctx, DyObject left, DyObject right)
    {
        try
        {
            return DyTuple.Lesser(ctx, ((DyClass)left).Fields, ((DyClass)right).Fields);
        }
        catch (DyCodeException e)
        {
            ctx.Error = e.Error;
            return Nil;
        }
    }
}
