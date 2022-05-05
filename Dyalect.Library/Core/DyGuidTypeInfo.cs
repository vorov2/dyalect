using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyGuidTypeInfo : DyForeignTypeInfo<CoreModule>
{
    private const string GuidType = "Guid";

    public override string ReflectedTypeName => GuidType;

    public DyGuidTypeInfo() => AddMixins(Dy.Order);

    #region Operations
    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString("{" + arg.ToString().ToUpper() + "}");

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return False;

        return ((DyGuid)left).Value == ((DyGuid)right).Value ? True : False;
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return (DyBool)(((DyGuid)left).Value.CompareTo(((DyGuid)right).Value) > 0);
    }

    protected override DyObject GteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return (DyBool)(((DyGuid)left).Value.CompareTo(((DyGuid)right).Value) >= 0);
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return (DyBool)(((DyGuid)left).Value.CompareTo(((DyGuid)right).Value) < 0);
    }

    protected override DyObject LteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return (DyBool)(((DyGuid)left).Value.CompareTo(((DyGuid)right).Value) <= 0);
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType)
    {
        if (targetType.ReflectedTypeId == Dy.String)
            return self.ToString(ctx);
        else if (targetType.ReflectedTypeId == DeclaringUnit.ByteArray.ReflectedTypeId)
            return ToByteArray(ctx, (DyGuid)self);
        else
            return base.CastOp(ctx, self, targetType);
    }
    #endregion

    [InstanceMethod]
    internal static DyObject ToByteArray(ExecutionContext ctx, DyGuid self) =>
        new DyByteArray(ctx.Type<DyByteArrayTypeInfo>(), self.Value.ToByteArray());

    [StaticMethod]
    internal static DyObject Parse(ExecutionContext ctx, string value)
    {
        try
        {
            return new DyGuid(ctx.Type<DyGuidTypeInfo>(), Guid.Parse(value));
        }
        catch (FormatException)
        {
            return ctx.InvalidValue(value);
        }
    }

    [StaticMethod]
    internal static DyObject FromByteArray(ExecutionContext ctx, DyByteArray value)
    {
        try
        {
            return new DyGuid(ctx.Type<DyGuidTypeInfo>(), new(value.GetBytes()));
        }
        catch (ArgumentException)
        {
            return ctx.InvalidValue(value);
        }
    }

    [StaticMethod(GuidType)]
    internal static DyObject NewGuid(ExecutionContext ctx) => new DyGuid(ctx.Type<DyGuidTypeInfo>(), Guid.NewGuid());

    [StaticProperty]
    internal static DyObject Default(ExecutionContext ctx) => new DyGuid(ctx.Type<DyGuidTypeInfo>(), Guid.Empty);

    [StaticProperty]
    internal static DyObject Empty(ExecutionContext ctx) => Default(ctx);
}
