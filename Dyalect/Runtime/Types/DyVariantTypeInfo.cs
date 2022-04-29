using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal sealed class DyVariantTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Variant);

    public override int ReflectedTypeId => Dy.Variant;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Len | SupportedOperations.Get | SupportedOperations.Set;

    public DyVariantTypeInfo() => AddMixin(Dy.Collection);

    #region Operations
    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId || left.GetConstructor() != right.GetConstructor())
            return DyBool.False;

        var (xs, ys) = ((DyVariant)left, (DyVariant)right);
        return ctx.RuntimeContext.Tuple.Eq(ctx, xs.Tuple, ys.Tuple);
    }

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg) =>
        DyInteger.Get(((DyVariant)arg).Tuple.Count);

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        var self = (DyVariant)arg;
        var str = ctx.RuntimeContext.Tuple.ToStringDirect(ctx, self.Tuple);

        if (ctx.HasErrors)
            return DyNil.Instance;

        return new DyString($"{ReflectedTypeName}.{self.Constructor}{str}");
    }

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg)
    {
        var self = (DyVariant)arg;
        var str = ctx.RuntimeContext.Tuple.ToLiteralDirect(ctx, self.Tuple);

        if (ctx.HasErrors)
            return DyNil.Instance;

        return new DyString($"@{self.Constructor}{str}");
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) =>
        ctx.RuntimeContext.Tuple.GetDirect(ctx, ((DyVariant)self).Tuple, index);

    protected override DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value) =>
        ctx.RuntimeContext.Tuple.Set(ctx, ((DyVariant)self).Tuple, index, value);

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Tuple => GetTuple(ctx, (DyVariant)self),
            _ => base.CastOp(ctx, self, targetType)
        };

    private DyObject GetTuple(ExecutionContext ctx, DyVariant self)
    {
        if (self.Tuple.Count == 0)
            return ctx.InvalidCast(ReflectedTypeName, nameof(Dy.Tuple));

        return self.Tuple;
    }
    #endregion

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx)
    {
        if (!char.IsUpper(name[0]))
            return base.InitializeStaticMember(name, ctx);

        return new DyVariantConstructor(name, (_, args) => new DyVariant(name, args), new("values", ParKind.VarArg));
    }
}
