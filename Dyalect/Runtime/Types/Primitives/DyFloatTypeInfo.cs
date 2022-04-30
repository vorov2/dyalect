using Dyalect.Codegen;
using System.Globalization;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyFloatTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
        | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
        | SupportedOperations.Neg | SupportedOperations.Plus;

    public override string ReflectedTypeName => nameof(Dy.Float);

    public override int ReflectedTypeId => Dy.Float;

    public DyFloatTypeInfo() => AddMixin(Dy.Number, Dy.Comparable);

    #region Binary Operations
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() + right.GetFloat());

        if (right.TypeId == Dy.String)
            return left.Concat(right, ctx);

        return base.AddOp(ctx, left, right);
    }

    protected override DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() - right.GetFloat());

        return base.SubOp(ctx, left, right);
    }

    protected override DyObject MulOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() * right.GetFloat());

        return base.MulOp(ctx, left, right);
    }

    protected override DyObject DivOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() / right.GetFloat());

        return base.DivOp(ctx, left, right);
    }

    protected override DyObject RemOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return new DyFloat(left.GetFloat() % right.GetFloat());

        return base.RemOp(ctx, left, right);
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() == right.GetFloat() ? True : False;

        return base.EqOp(ctx, left, right); //Important! Should redirect to base
    }

    protected override DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() != right.GetFloat() ? True : False;

        return base.NeqOp(ctx, left, right); //Important! Should redirect to base
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() > right.GetFloat() ? True : False;

        return base.GtOp(ctx, left, right);
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() < right.GetFloat() ? True : False;

        return base.LtOp(ctx, left, right);
    }

    protected override DyObject GteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() >= right.GetFloat() ? True : False;

        return base.GteOp(ctx, left, right);
    }

    protected override DyObject LteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Float || right.TypeId == Dy.Integer)
            return left.GetFloat() <= right.GetFloat() ? True : False;

        return base.LteOp(ctx, left, right);
    }

    protected override DyObject NegOp(ExecutionContext ctx, DyObject arg) => new DyFloat(-arg.GetFloat());

    protected override DyObject PlusOp(ExecutionContext ctx, DyObject arg) => arg;

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        var f = arg.GetFloat();
        return new DyString(f.ToString(SystemCulture.NumberFormat));
    }

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg)
    {
        var f = arg.GetFloat();
        return new DyString(f.ToString(InvariantCulture.NumberFormat));
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Integer => DyInteger.Get((long)self.GetFloat()),
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

    [InstanceMethod]
    internal static bool IsNaN(double self) => double.IsNaN(self);

    [StaticProperty]
    internal static DyObject Max() => DyFloat.Max;

    [StaticProperty]
    internal static DyObject Min() => DyFloat.Min;

    [StaticProperty]
    internal static DyObject Infinity() => DyFloat.PositiveInfinity;

    [StaticProperty]
    internal static DyObject Default() => DyFloat.Zero;

    [StaticMethod]
    internal static double? Parse(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, InvariantCulture.NumberFormat, out var i))
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
