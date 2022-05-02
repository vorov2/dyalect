namespace Dyalect.Runtime.Types;

internal sealed class DyLabelTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Label);

    public override int ReflectedTypeId => Dy.Label;

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.In;

    protected override DyObject ContainsOp(ExecutionContext ctx, DyObject self, DyObject field) =>
        field.TypeId == Dy.String && ((DyLabel)self).Label == field.GetString() ? True : False;

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        var lab = (DyLabel)arg;
        return new DyString(lab.Label + ": " + lab.Value.ToString(ctx).Value);
    }
}
