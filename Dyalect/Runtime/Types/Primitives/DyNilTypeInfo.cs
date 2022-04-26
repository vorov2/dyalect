using Dyalect.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyNilTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(DyType.Nil);

    public override int ReflectedTypeId => DyType.Nil;

    #region Operations
    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        left.TypeId == right.TypeId ? DyBool.True : DyBool.False;

    protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => DyBool.True;

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => new DyString(DyNil.Literal);

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, DyNil.Instance, ctx);

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Bool => DyBool.False,
            _ => base.CastOp(self, targetType, ctx)
        };
    #endregion

    [StaticMethod(Method.Nil)] 
    internal static DyNil GetNil() => DyNil.Instance;

    [StaticMethod]
    internal static DyNil Default() => DyNil.Instance;
}
