namespace Dyalect.Runtime.Types;

internal sealed class DyLabelTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

    public override string TypeName => DyTypeNames.Label;

    public override int ReflectedTypeId => DyType.Label;

    protected override DyObject ContainsOp(DyObject self, DyObject field, ExecutionContext ctx) =>
        field.TypeId == DyType.String && self.GetLabel() == field.GetString() ? DyBool.True : DyBool.False;

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
    {
        var lab = (DyLabel)arg;
        return new DyString(lab.Label + ": " + lab.Value.ToString(ctx).Value);
    }
}
