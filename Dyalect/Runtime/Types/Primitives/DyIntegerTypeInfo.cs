﻿using Dyalect.Codegen;
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

    public DyIntegerTypeInfo() => AddMixin(Dy.Number, Dy.Comparable);

    #region Binary Operations
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return new DyInteger(left.GetInteger() + right.GetInteger());

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() + right.GetFloat());

        if (right.TypeId == Dy.String)
            return ctx.RuntimeContext.String.Add(ctx, left, right);

        return base.AddOp(ctx, left, right);
    }

    protected override DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return new DyInteger(left.GetInteger() - right.GetInteger());

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() - right.GetFloat());

        return base.SubOp(ctx, left, right);
    }

    protected override DyObject MulOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return new DyInteger(left.GetInteger() * right.GetInteger());

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() * right.GetFloat());

        return base.MulOp(ctx, left, right);
    }

    protected override DyObject DivOp(ExecutionContext ctx, DyObject left, DyObject right)
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

        return base.DivOp(ctx, left, right);
    }

    protected override DyObject RemOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return new DyInteger(left.GetInteger() % right.GetInteger());

        if (right.TypeId == Dy.Float)
            return new DyFloat(left.GetFloat() % right.GetFloat());

        return base.RemOp(ctx, left, right);
    }

    protected override DyObject ShiftLeftOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.ShiftLeftOp(ctx, left, right);
        return new DyInteger(left.GetInteger() << (int)right.GetInteger());
    }

    protected override DyObject ShiftRightOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.ShiftRightOp(ctx, left, right);
        return new DyInteger(left.GetInteger() >> (int)right.GetInteger());
    }

    protected override DyObject AndOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.AndOp(ctx, left, right);
        return new DyInteger(left.GetInteger() & (int)right.GetInteger());
    }

    protected override DyObject OrOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.OrOp(ctx, left, right);
        return new DyInteger((int)left.GetInteger() | (int)right.GetInteger());
    }

    protected override DyObject XorOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return base.XorOp(ctx, left, right);
        return new DyInteger(left.GetInteger() ^ (int)right.GetInteger());
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() == right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() == right.GetFloat() ? True : False;

        return base.EqOp(ctx, left, right); //Important! Should redirect to base
    }

    protected override DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() != right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() != right.GetFloat() ? True : False;

        return base.NeqOp(ctx, left, right); //Important! Should redirect to base
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() > right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() > right.GetFloat() ? True : False;

        return base.GtOp(ctx, left, right);
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() < right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() < right.GetFloat() ? True : False;

        return base.LtOp(ctx, left, right);
    }

    protected override DyObject GteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() >= right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() >= right.GetFloat() ? True : False;

        return base.GteOp(ctx, left, right);
    }

    protected override DyObject LteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return left.GetInteger() <= right.GetInteger() ? True : False;

        if (right.TypeId == Dy.Float)
            return left.GetFloat() <= right.GetFloat() ? True : False;

        return base.LteOp(ctx, left, right);
    }

    protected override DyObject NegOp(ExecutionContext ctx, DyObject arg) => new DyInteger(-arg.GetInteger());

    protected override DyObject PlusOp(ExecutionContext ctx, DyObject arg) => arg;

    protected override DyObject BitwiseNotOp(ExecutionContext ctx, DyObject arg) => new DyInteger(~arg.GetInteger());

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString(arg.GetInteger().ToString(CI.NumberFormat));
    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) => ToStringOp(ctx, arg, DyNil.Instance);

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Float => new DyFloat(self.GetInteger()),
            _ => base.CastOp(ctx, self, targetType)
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
