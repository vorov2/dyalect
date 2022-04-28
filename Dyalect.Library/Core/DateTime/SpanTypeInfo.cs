using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Library.Core;

public abstract class SpanTypeInfo<T> : DyForeignTypeInfo<CoreModule>
    where T : DyObject, ISpan, IFormattable
{
    public override string ReflectedTypeName { get; }

    protected SpanTypeInfo(string typeName)
    {
        ReflectedTypeName = typeName;
        AddMixin(Dy.Comparable);
    }

    protected override SupportedOperations GetSupportedOperations() =>
       SupportedOperations.Gt | SupportedOperations.Gte | SupportedOperations.Lt | SupportedOperations.Lte;

    #region Operations
    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        if (format.Is(Dy.Nil))
            return new DyString(arg.ToString());

        if (!format.Is(ctx, Dy.String))
            return Nil;

        try
        {
            return new DyString(((T)arg).ToString(format.GetString(), null));
        }
        catch (FormatException)
        {
            return ctx.ParsingFailed();
        }
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId != left.TypeId)
            return DyBool.False;

        return ((T)left).TotalTicks == ((T)right).TotalTicks ? True : False;
    }

    protected override DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId != left.TypeId)
            return DyBool.True;

        return ((T)left).TotalTicks != ((T)right).TotalTicks ? True : False;
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId != left.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return ((T)left).TotalTicks > ((T)right).TotalTicks ? True : False;
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId != left.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return ((T)left).TotalTicks < ((T)right).TotalTicks ? True : False;
    }

    protected override DyObject GteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId != left.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return ((T)left).TotalTicks >= ((T)right).TotalTicks ? True : False;
    }

    protected override DyObject LteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId != left.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return ((T)left).TotalTicks <= ((T)right).TotalTicks ? True : False;
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
       targetType.ReflectedTypeId switch
       {
           Dy.Integer => DyInteger.Get(((T)self).ToInteger()),
           _ => base.CastOp(ctx, self, targetType)
       };
    #endregion
}
