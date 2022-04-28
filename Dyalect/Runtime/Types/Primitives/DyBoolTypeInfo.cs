using Dyalect.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyBoolTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(Dy.Bool);

    public override int ReflectedTypeId => Dy.Bool;

    #region Operations
    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ReferenceEquals(left, right) ? True : False;

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString(ReferenceEquals(arg, True) ? "true" : "false");

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) => ToStringOp(ctx, arg, DyNil.Instance);

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Integer => ReferenceEquals(self, True) ? DyInteger.One : DyInteger.Zero,
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

    [StaticMethod(Method.Bool)] 
    internal static bool CreateBool(DyObject value) => value.IsTrue();

    [StaticProperty]
    internal static DyBool Default() => False;

    [StaticProperty]
    internal static DyBool Max() => True;

    [StaticProperty]
    internal static DyBool Min() => False;
}
