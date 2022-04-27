namespace Dyalect.Runtime.Types;

internal sealed class DyObjectTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

    public override string ReflectedTypeName => nameof(Dy.Object);

    public override int ReflectedTypeId => Dy.Object;

    public DyObjectTypeInfo() => Closed = true;
}
