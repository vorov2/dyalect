namespace Dyalect.Runtime.Types;

internal sealed class DyMetaTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

    public override string ReflectedTypeName => nameof(Dy.TypeInfo);

    public override int ReflectedTypeId => Dy.TypeInfo;

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (index.TypeId == Dy.String)
            return index.GetString() switch
            {
                "name" => new DyString(((DyTypeInfo)self).ReflectedTypeName),
                _ => ctx.IndexOutOfRange(index)
            };

        return ctx.IndexOutOfRange();
    }

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString("TypeInfo<" + ((DyTypeInfo)arg).ReflectedTypeName + ">");
}
