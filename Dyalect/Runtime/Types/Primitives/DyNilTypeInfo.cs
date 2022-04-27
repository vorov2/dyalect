using Dyalect.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyNilTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(Dy.Nil);

    public override int ReflectedTypeId => Dy.Nil;

    #region Operations
    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        left.TypeId == right.TypeId ? True : False;

    protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => True;

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => new DyString(DyNil.Literal);

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, DyNil.Instance, ctx);

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Bool => False,
            _ => base.CastOp(self, targetType, ctx)
        };
    #endregion

    [StaticMethod(Method.Nil)] 
    internal static DyNil GetNil() => Nil;

    [StaticMethod]
    internal static DyNil Default() => Nil;
}
