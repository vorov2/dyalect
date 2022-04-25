using Dyalect.Debug;
using Dyalect.Runtime.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyBoolTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(DyType.Bool);

    public override int ReflectedTypeId => DyType.Bool;

    public DyBoolTypeInfo() => AddMixin(DyType.Bounded);

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ReferenceEquals(left, right) ? DyBool.True : DyBool.False;

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
        new DyString(ReferenceEquals(arg, DyBool.True) ? "true" : "false");

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, DyNil.Instance, ctx);

    [StaticMethod] bool Bool(bool val) => val;

    [StaticMethod] DyBool Default() => DyBool.False;

    [StaticMethod] DyBool Max() => DyBool.True;

    [StaticMethod] DyBool Min() => DyBool.False;

    //protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
    //    name switch
    //    {
    //        Method.Bool => Func.Static(name, Convert, -1, new Par("value")),
    //        Method.Default => Func.Static(name, _ => DyBool.False),
    //        Method.Min => Func.Static(name, _ => DyBool.False),
    //        Method.Max => Func.Static(name, _ => DyBool.True),
    //        _ => base.InitializeStaticMember(name, ctx)
    //    };

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Integer => ReferenceEquals(self, DyBool.True) ? DyInteger.One : DyInteger.Zero,
            _ => base.CastOp(self, targetType, ctx)
        };
}
