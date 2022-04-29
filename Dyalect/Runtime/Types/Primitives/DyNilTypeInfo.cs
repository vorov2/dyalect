using Dyalect.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyNilTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Nil);

    public override int ReflectedTypeId => Dy.Nil;

    #region Operations
    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        left.TypeId == right.TypeId ? True : False;

    protected override DyObject NotOp(ExecutionContext ctx, DyObject arg) => True;

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) => new DyString(DyNil.Literal);

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) => ToStringOp(ctx, arg, DyNil.Instance);

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
