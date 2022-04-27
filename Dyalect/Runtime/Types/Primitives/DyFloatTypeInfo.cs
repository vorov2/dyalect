using Dyalect.Codegen;
using System.Globalization;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyFloatTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
        | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
        | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
        | SupportedOperations.Neg | SupportedOperations.Plus | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(Dy.Float);

    public override int ReflectedTypeId => Dy.Float;

    public DyFloatTypeInfo() => AddMixin(Dy.Number, Dy.Comparable, Dy.Bounded);

    #region Binary Operations
    protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() + right.GetFloat());

        if (right.TypeId == Dy.String)
            return ctx.RuntimeContext.Types[Dy.String].Add(ctx, left, right);

        return base.AddOp(left, right, ctx);
    }

    protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() - right.GetFloat());

        return base.SubOp(left, right, ctx);
    }

    protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() * right.GetFloat());

        return base.MulOp(left, right, ctx);
    }

    protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() / right.GetFloat());

        return base.DivOp(left, right, ctx);
    }

    protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() % right.GetFloat());

        return base.RemOp(left, right, ctx);
    }

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() == right.GetFloat() ? True : False;

        return base.EqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() != right.GetFloat() ? True : False;

        return base.NeqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() > right.GetFloat() ? True : False;

        return base.GtOp(left, right, ctx);
    }

    protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() < right.GetFloat() ? True : False;

        return base.LtOp(left, right, ctx);
    }

    protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() >= right.GetFloat() ? True : False;

        return base.GteOp(left, right, ctx);
    }

    protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() <= right.GetFloat() ? True : False;

        return base.LteOp(left, right, ctx);
    }

    protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyFloat(-arg.GetFloat());

    protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
    {
        var f = arg.GetFloat();
        return new DyString(f.ToString(CI.NumberFormat));
    }

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, DyNil.Instance, ctx);

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Integer => DyInteger.Get((long)self.GetFloat()),
            _ => base.CastOp(self, targetType, ctx)
        };
    #endregion

    [InstanceMethod]
    internal static bool IsNaN(double self) => double.IsNaN(self);

    [StaticMethod]
    internal static DyObject Max() => DyFloat.Max;

    [StaticMethod]
    internal static DyObject Min() => DyFloat.Min;

    [StaticMethod]
    internal static DyObject Inf() => DyFloat.PositiveInfinity;

    [StaticMethod]
    internal static DyObject Default() => DyFloat.Zero;

    [StaticMethod]
    internal static double? Parse(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, CI.NumberFormat, out var i))
            return i;
        return null;
    }

    [StaticMethod(Method.Float)]
    internal static double? Convert(ExecutionContext ctx, DyObject value)
    {
        if (value.TypeId is Dy.Float)
            return value.GetFloat();

        if (value.TypeId is Dy.Integer)
            return value.GetInteger();

        if (value.TypeId is Dy.Char or Dy.String)
            return Parse(value.GetString());

        ctx.InvalidType(Dy.Float, Dy.Integer, Dy.Char, Dy.String, value);
        return default;
    }
}
