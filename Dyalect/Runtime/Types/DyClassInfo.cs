namespace Dyalect.Runtime.Types;

internal sealed class DyClassInfo : DyTypeInfo
{
    private readonly bool privateCons;

    public override string ReflectedTypeName { get; }

    public override int ReflectedTypeId { get; }

    public DyClassInfo(string typeName, int typeCode)
    {
        ReflectedTypeName = typeName;
        ReflectedTypeId = typeCode;
        privateCons = !string.IsNullOrEmpty(typeName) && typeName.Length > 0 && char.IsLower(typeName[0]);
    }

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len;

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        var self = (DyClass)left;

        if (self.TypeId == right.TypeId && right is DyClass t && t.Constructor == self.Constructor)
        {
            var res = self.Fields.Equals(t.Fields, ctx);

            if (ctx.HasErrors)
                return Nil;

            return res ? True : False;
        }

        return False;
    }

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        ToStringOrLiteral(ctx, arg, false);

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) =>
        ToStringOrLiteral(ctx, arg, true);

    private DyObject ToStringOrLiteral(ExecutionContext ctx, DyObject arg, bool literal)
    {
        var cust = (DyClass)arg;
        var priv = cust.Fields;

        if (ReflectedTypeName == cust.Constructor && priv.Count == 0)
            return new DyString($"{ReflectedTypeName}()");
        else if (ReflectedTypeName == cust.Constructor)
            return new DyString($"{ReflectedTypeName}{(literal ? priv.ToLiteral(ctx) : priv.ToString(ctx))}");
        else if (priv.Count == 0)
            return new DyString($"{ReflectedTypeName}.{cust.Constructor}()");
        else
            return new DyString($"{ReflectedTypeName}.{cust.Constructor}{(literal ? priv.ToLiteral(ctx) : priv.ToString(ctx))}");
    }

    protected override DyObject ContainsOp(ExecutionContext ctx, DyObject self, DyObject field) =>
        ((DyClass)self).Fields.GetOrdinal(field.GetString()) is not -1 ? True : False;

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject self)
    {
        var cls = (DyClass)self;

        if (privateCons && ctx.UnitId != cls.DeclaringUnit.Id)
            return base.LengthOp(ctx, self);

        return DyInteger.Get(cls.Fields.Count);
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index)
    {
        var cls = (DyClass)self;

        if (privateCons && ctx.UnitId != cls.DeclaringUnit.Id)
            return base.GetOp(ctx, self, index);

        return cls.Fields.GetItem(index, ctx);
    }

    protected override DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        var cls = (DyClass)self;

        if (privateCons && ctx.UnitId != cls.DeclaringUnit.Id)
            return base.SetOp(ctx, self, index, value);

        cls.Fields.SetItem(index, value, ctx);
        return Nil;
    }
}
