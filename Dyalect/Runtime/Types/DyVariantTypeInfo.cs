using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal sealed class DyVariantTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(DyType.Variant);

    public override int ReflectedTypeId => DyType.Variant;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Len | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Lit;

    public DyVariantTypeInfo() => AddMixin(DyType.Collection);

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId != right.TypeId || left.GetConstructor() != right.GetConstructor())
            return DyBool.False;

        var (xs, ys) = ((DyVariant)left, (DyVariant)right);
        return ctx.RuntimeContext.Tuple.Eq(ctx, xs.Tuple, ys.Tuple);
    }

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
        DyInteger.Get(((DyVariant)arg).Tuple.Count);

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
    {
        var self = (DyVariant)arg;
        var str = ctx.RuntimeContext.Tuple.ToStringDirect(ctx, self.Tuple);

        if (ctx.HasErrors)
            return DyNil.Instance;

        return new DyString($"{ReflectedTypeName}.{self.Constructor}{str}");
    }

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx)
    {
        var self = (DyVariant)arg;
        var str = ctx.RuntimeContext.Tuple.ToLiteralDirect(ctx, self.Tuple);

        if (ctx.HasErrors)
            return DyNil.Instance;

        return new DyString($"@{self.Constructor}{str}");
    }

    protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) =>
        ctx.RuntimeContext.Tuple.GetDirect(ctx, ((DyVariant)self).Tuple, index);

    protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx) =>
        ctx.RuntimeContext.Tuple.Set(ctx, ((DyVariant)self).Tuple, index, value);

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx)
    {
        if (!char.IsUpper(name[0]))
            return base.InitializeStaticMember(name, ctx);

        return Func.Constructor(name, (DyTuple args) => new DyVariant(name, args), new("values", ParKind.VarArg));
    }

    private DyObject GetTuple(ExecutionContext ctx, DyVariant self)
    {
        if (self.Tuple.Count == 0)
            return ctx.InvalidCast(ReflectedTypeName, nameof(DyType.Tuple));

        return self.Tuple;
    }

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Tuple => GetTuple(ctx, (DyVariant)self),
            _ => base.CastOp(self, targetType, ctx)
        };
}
