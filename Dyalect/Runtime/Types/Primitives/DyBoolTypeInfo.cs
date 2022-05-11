using Dyalect.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyBoolTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Bool);

    public override int ReflectedTypeId => Dy.Bool;

    public DyBoolTypeInfo() => AddMixins(Dy.Show);

    #region Operations
    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString(ReferenceEquals(arg, True) ? "true" : "false");

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
