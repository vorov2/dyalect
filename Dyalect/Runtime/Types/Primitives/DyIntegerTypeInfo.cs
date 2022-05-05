using Dyalect.Codegen;
using System.Globalization;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyIntegerTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
        | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
        | SupportedOperations.Neg | SupportedOperations.Plus | SupportedOperations.And | SupportedOperations.Or
        | SupportedOperations.Xor | SupportedOperations.BitNot | SupportedOperations.Shl | SupportedOperations.Shr;

    public override string ReflectedTypeName => nameof(Dy.Integer);

    public override int ReflectedTypeId => Dy.Integer;

    public DyIntegerTypeInfo() => AddMixins(Dy.Number, Dy.Order);

    #region Binary Operations
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return new DyInteger(((DyInteger)left).Value + i8.Value);

        if (right is DyFloat r8)
            return new DyFloat(((DyInteger)left).Value + r8.Value);

        return base.AddOp(ctx, left, right);
    }

    protected override DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return new DyInteger(((DyInteger)left).Value - i8.Value);

        if (right is DyFloat r8)
            return new DyFloat(((DyInteger)left).Value - r8.Value);

        return base.SubOp(ctx, left, right);
    }

    protected override DyObject MulOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return new DyInteger(((DyInteger)left).Value * i8.Value);

        if (right is DyFloat r8)
            return new DyFloat(((DyInteger)left).Value * r8.Value);

        return base.MulOp(ctx, left, right);
    }

    protected override DyObject DivOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
        {
            if (i8.Value == 0)
                return ctx.DivideByZero();

            return new DyInteger(((DyInteger)left).Value / i8.Value);
        }

        if (right is DyFloat r8)
            return new DyFloat(((DyInteger)left).Value / r8.Value);

        return base.DivOp(ctx, left, right);
    }

    protected override DyObject RemOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return new DyInteger(((DyInteger)left).Value % i8.Value);

        if (right is DyFloat r8)
            return new DyFloat(((DyInteger)left).Value % r8.Value);

        return base.RemOp(ctx, left, right);
    }

    protected override DyObject ShiftLeftOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.ShiftLeftOp(ctx, left, right);
        return new DyInteger(((DyInteger)left).Value << (int)((DyInteger)right).Value);
    }

    protected override DyObject ShiftRightOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.ShiftRightOp(ctx, left, right);
        return new DyInteger(((DyInteger)left).Value >> (int)((DyInteger)right).Value);
    }

    protected override DyObject AndOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.AndOp(ctx, left, right);
        return new DyInteger(((DyInteger)left).Value & (int)((DyInteger)right).Value);
    }

    protected override DyObject OrOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.OrOp(ctx, left, right);
        return new DyInteger((int)((DyInteger)left).Value | (int)((DyInteger)right).Value);
    }

    protected override DyObject XorOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.XorOp(ctx, left, right);
        return new DyInteger(((DyInteger)left).Value ^ (int)((DyInteger)right).Value);
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return ((DyInteger)left).Value == i8.Value ? True : False;

        if (right is DyFloat r8)
            return ((DyInteger)left).Value == r8.Value ? True : False;

        return base.EqOp(ctx, left, right);
    }

    protected override DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return ((DyInteger)left).Value != i8.Value ? True : False;

        if (right is DyFloat r8)
            return ((DyInteger)left).Value != r8.Value ? True : False;

        return base.NeqOp(ctx, left, right);
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return ((DyInteger)left).Value > i8.Value ? True : False;

        if (right is DyFloat r8)
            return ((DyInteger)left).Value > r8.Value ? True : False;

        return base.GtOp(ctx, left, right);
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return ((DyInteger)left).Value < i8.Value ? True : False;

        if (right is DyFloat r8)
            return ((DyInteger)left).Value < r8.Value ? True : False;

        return base.LtOp(ctx, left, right);
    }

    protected override DyObject GteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return ((DyInteger)left).Value >= i8.Value ? True : False;

        if (right is DyFloat r8)
            return ((DyInteger)left).Value >= r8.Value ? True : False;

        return base.GteOp(ctx, left, right);
    }

    protected override DyObject LteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i8)
            return ((DyInteger)left).Value <= i8.Value ? True : False;

        if (right is DyFloat r8)
            return ((DyInteger)left).Value <= r8.Value ? True : False;

        return base.LteOp(ctx, left, right);
    }

    protected override DyObject NegOp(ExecutionContext ctx, DyObject arg) => new DyInteger(-((DyInteger)arg).Value);

    protected override DyObject PlusOp(ExecutionContext ctx, DyObject arg) => arg;

    protected override DyObject BitwiseNotOp(ExecutionContext ctx, DyObject arg) => new DyInteger(~((DyInteger)arg).Value);

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString(((DyInteger)arg).Value.ToString(SystemCulture.NumberFormat));

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) =>
        new DyString(((DyInteger)arg).Value.ToString(InvariantCulture.NumberFormat));

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Float => new DyFloat(((DyInteger)self).Value),
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

    [InstanceMethod]
    internal static bool IsMultipleOf(long self, long value) => (self % value) == 0;

    [StaticMethod]
    internal static long? Parse(string value)
    {
        if (long.TryParse(value, NumberStyles.Integer, InvariantCulture.NumberFormat, out var i))
            return i;
        return default;
    }

    [StaticMethod(Method.Integer)]
    internal static long? CreateNew(DyObject value)
    {
        if (value is DyInteger i8)
            return i8.Value;

        if (value is DyFloat r8)
            return (long)r8.Value;

        if (value.TypeId is Dy.Char or Dy.String)
            return Parse(value.ToString());

        throw new DyCodeException(DyError.InvalidType, value);
    }

    [StaticProperty] 
    internal static DyObject Max() => DyInteger.Max;

    [StaticProperty] 
    internal static DyObject Min() => DyInteger.Min;

    [StaticProperty] 
    internal static DyObject Default() => DyInteger.Zero;
}
