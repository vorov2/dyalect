using Dyalect.Codegen;
using System.Globalization;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyIntegerTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
        | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
        | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
        | SupportedOperations.Neg | SupportedOperations.Plus | SupportedOperations.And | SupportedOperations.Or
        | SupportedOperations.Xor | SupportedOperations.BitNot | SupportedOperations.Shl | SupportedOperations.Shr;

    public override string ReflectedTypeName => nameof(Dy.Integer);

    public override int ReflectedTypeId => Dy.Integer;

    public DyIntegerTypeInfo() => AddMixin(Dy.Number, Dy.Comparable, Dy.Bounded);

    #region Binary Operations
    protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return new DyInteger(left.GetInteger() + right.GetInteger());

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() + right.GetFloat());

        if (right.TypeId == Dy.String)
            return ctx.RuntimeContext.String.Add(ctx, left, right);

        return base.AddOp(left, right, ctx);
    }

    protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return new DyInteger(left.GetInteger() - right.GetInteger());

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() - right.GetFloat());

        return base.SubOp(left, right, ctx);
    }

    protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return new DyInteger(left.GetInteger() * right.GetInteger());

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() * right.GetFloat());

        return base.MulOp(left, right, ctx);
    }

    protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
        {
            var i = right.GetInteger();

            if (i == 0)
                return ctx.DivideByZero();

            return new DyInteger(left.GetInteger() / i);
        }

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() / right.GetFloat());

        return base.DivOp(left, right, ctx);
    }

    protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return new DyInteger(left.GetInteger() % right.GetInteger());

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() % right.GetFloat());

        return base.RemOp(left, right, ctx);
    }

    protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId != right.TypeId)
            return base.ShiftLeftOp(left, right, ctx);
        return new DyInteger(left.GetInteger() << (int)right.GetInteger());
    }

    protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId != right.TypeId)
            return base.ShiftRightOp(left, right, ctx);
        return new DyInteger(left.GetInteger() >> (int)right.GetInteger());
    }

    protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId != right.TypeId)
            return base.AndOp(left, right, ctx);
        return new DyInteger(left.GetInteger() & (int)right.GetInteger());
    }

    protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId != right.TypeId)
            return base.OrOp(left, right, ctx);
        return new DyInteger((int)left.GetInteger() | (int)right.GetInteger());
    }

    protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId != right.TypeId)
            return base.XorOp(left, right, ctx);
        return new DyInteger(left.GetInteger() ^ (int)right.GetInteger());
    }

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() == right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() == right.GetFloat() ? True : False;

        return base.EqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() != right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() != right.GetFloat() ? True : False;

        return base.NeqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() > right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() > right.GetFloat() ? True : False;

        return base.GtOp(left, right, ctx);
    }

    protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() < right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() < right.GetFloat() ? True : False;

        return base.LtOp(left, right, ctx);
    }

    protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() >= right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() >= right.GetFloat() ? True : False;

        return base.GteOp(left, right, ctx);
    }

    protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() <= right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() <= right.GetFloat() ? True : False;

        return base.LteOp(left, right, ctx);
    }

    protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyInteger(-arg.GetInteger());

    protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

    protected override DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) => new DyInteger(~arg.GetInteger());

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
        new DyString(arg.GetInteger().ToString(CI.NumberFormat));
    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, DyNil.Instance, ctx);

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Float => new DyFloat(self.GetInteger()),
            _ => base.CastOp(self, targetType, ctx)
        };
    #endregion

    [InstanceMethod]
    internal static bool IsMultipleOf(long self, long value) => (self % value) == 0;

    [StaticMethod]
    internal static DyObject Integer(ExecutionContext ctx, DyObject obj)
    {
        if (obj.TypeId == Dy.Integer)
            return obj;

        if (obj.TypeId == Dy.Float)
            return DyInteger.Get((long)obj.GetFloat());

        if (obj.TypeId == Dy.Char || obj.TypeId == Dy.String)
        {
            _ = long.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i);
            return DyInteger.Get(i);
        }

        return ctx.InvalidType(Dy.Integer, Dy.Float, obj);
    }

    [StaticMethod]
    internal static DyObject Parse(DyObject obj)
    {
        if (obj.TypeId == Dy.Integer)
            return obj;

        if (obj.TypeId == Dy.Float)
            return DyInteger.Get((long)obj.GetFloat());

        if ((obj.TypeId == Dy.Char || obj.TypeId == Dy.String) &&
            long.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i))
            return DyInteger.Get(i);

        return DyNil.Instance;
    }

    [StaticMethod] 
    internal static DyInteger Max() => DyInteger.Max;

    [StaticMethod] 
    internal static DyInteger Min() => DyInteger.Min;

    [StaticMethod] 
    internal static DyInteger Default() => DyInteger.Zero;
}
