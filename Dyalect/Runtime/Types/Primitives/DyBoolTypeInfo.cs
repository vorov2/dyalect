using Dyalect.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyBoolTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(Dy.Bool);

    public override int ReflectedTypeId => Dy.Bool;

    public DyBoolTypeInfo() => AddMixin(Dy.Bounded);

    #region Operations
    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ReferenceEquals(left, right) ? True : False;

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
        new DyString(ReferenceEquals(arg, True) ? "true" : "false");

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, DyNil.Instance, ctx);

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Integer => ReferenceEquals(self, True) ? DyInteger.One : DyInteger.Zero,
            _ => base.CastOp(self, targetType, ctx)
        };
    #endregion

    [StaticMethod(Method.Bool)] 
    internal static bool CreateBool(DyObject value) => value.IsTrue();

    [StaticMethod] internal static DyBool Default() => False;

    [StaticMethod] internal static DyBool Max() => True;

    [StaticMethod] internal static DyBool Min() => False;
}
