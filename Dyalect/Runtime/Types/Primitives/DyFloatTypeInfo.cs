using Dyalect.Codegen;
using System.Globalization;

namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyFloatTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Float);

    public override int ReflectedTypeId => Dy.Float;

    public DyFloatTypeInfo() => AddMixins(Dy.Number, Dy.Order, Dy.Show, Dy.Equatable);

    #region Operations
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return new DyFloat(((DyFloat)left).Value + ((DyFloat)right).Value);

        if (right.TypeId is Dy.Integer)
            return new DyFloat(((DyFloat)left).Value + ((DyInteger)right).Value);

        return base.AddOp(ctx, left, right);
    }

    protected override DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return new DyFloat(((DyFloat)left).Value - ((DyFloat)right).Value);

        if (right.TypeId is Dy.Integer)
            return new DyFloat(((DyFloat)left).Value - ((DyInteger)right).Value);

        return base.SubOp(ctx, left, right);
    }

    protected override DyObject MulOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return new DyFloat(((DyFloat)left).Value * ((DyFloat)right).Value);

        if (right.TypeId is Dy.Integer)
            return new DyFloat(((DyFloat)left).Value * ((DyInteger)right).Value);

        return base.MulOp(ctx, left, right);
    }

    protected override DyObject DivOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return new DyFloat(((DyFloat)left).Value / ((DyFloat)right).Value);

        if (right.TypeId is Dy.Integer)
            return new DyFloat(((DyFloat)left).Value / ((DyInteger)right).Value);

        return base.DivOp(ctx, left, right);
    }

    protected override DyObject RemOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return new DyFloat(((DyFloat)left).Value % ((DyFloat)right).Value);

        if (right.TypeId is Dy.Integer)
            return new DyFloat(((DyFloat)left).Value % ((DyInteger)right).Value);

        return base.RemOp(ctx, left, right);
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return ((DyFloat)left).Value == ((DyFloat)right).Value ? True : False;

        if (right.TypeId is Dy.Integer)
            return ((DyFloat)left).Value == ((DyInteger)right).Value ? True : False;
        
        return base.EqOp(ctx, left, right);
    }

    protected override DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return ((DyFloat)left).Value != ((DyFloat)right).Value ? True : False;

        if (right.TypeId is Dy.Integer)
            return ((DyFloat)left).Value != ((DyInteger)right).Value ? True : False;


        return base.NeqOp(ctx, left, right);
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return ((DyFloat)left).Value > ((DyFloat)right).Value ? True : False;

        if (right.TypeId is Dy.Integer)
            return ((DyFloat)left).Value > ((DyInteger)right).Value ? True : False;

        return base.GtOp(ctx, left, right);
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return ((DyFloat)left).Value < ((DyFloat)right).Value ? True : False;

        if (right.TypeId is Dy.Integer)
            return ((DyFloat)left).Value < ((DyInteger)right).Value ? True : False;


        return base.LtOp(ctx, left, right);
    }

    protected override DyObject GteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return ((DyFloat)left).Value >= ((DyFloat)right).Value ? True : False;

        if (right.TypeId is Dy.Integer)
            return ((DyFloat)left).Value >= ((DyInteger)right).Value ? True : False;

        return base.GteOp(ctx, left, right);
    }

    protected override DyObject LteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.Float)
            return ((DyFloat)left).Value <= ((DyFloat)right).Value ? True : False;

        if (right.TypeId is Dy.Integer)
            return ((DyFloat)left).Value <= ((DyInteger)right).Value ? True : False;

        return base.LteOp(ctx, left, right);
    }

    protected override DyObject NegOp(ExecutionContext ctx, DyObject arg) => new DyFloat(-((DyFloat)arg).Value);

    protected override DyObject PlusOp(ExecutionContext ctx, DyObject arg) => arg;

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject self, DyObject format)
    {
        if (format.TypeId is not Dy.String and not Dy.Char and not Dy.Nil)
            return ctx.InvalidType(format);

        try
        {
            var value = ((DyFloat)self).Value;
            return new DyString(format.TypeId is Dy.Nil
                ? value.ToString(SystemCulture.NumberFormat)
                : value.ToString(format.ToString(), SystemCulture.NumberFormat));
        }
        catch (FormatException)
        {
            return ctx.ParsingFailed();
        }
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Integer => DyInteger.Get((long)((DyFloat)self).Value),
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
        return default;
    }

    [StaticMethod(Method.Float)]
    internal static double? Convert(DyObject value)
    {
        if (value is DyFloat f)
            return f.Value;

        if (value is DyInteger i)
            return i.Value;

        if (value.TypeId is Dy.Char or Dy.String)
            return Parse(value.ToString());

        throw new DyCodeException(DyError.InvalidType, value);
    }
}
