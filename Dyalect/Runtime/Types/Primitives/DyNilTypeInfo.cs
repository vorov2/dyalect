﻿using Dyalect.Codegen;

namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyNilTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Nil);

    public override int ReflectedTypeId => Dy.Nil;

    public DyNilTypeInfo() => AddMixins(Dy.Show);

    #region Operations
    protected override DyObject NotOp(ExecutionContext ctx, DyObject arg) => True;

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Bool => False,
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

    [StaticMethod(Method.Nil)] 
    internal static DyNil GetNil() => Nil;

    [StaticProperty]
    internal static DyNil Default() => Nil;
}
