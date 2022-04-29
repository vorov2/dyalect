namespace Dyalect.Runtime.Types;

internal sealed class DyObjectTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Object);

    public override int ReflectedTypeId => Dy.Object;

    public DyObjectTypeInfo() => Closed = true;
}
