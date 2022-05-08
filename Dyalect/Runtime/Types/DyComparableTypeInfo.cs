using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyComparableTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Order);

    public override int ReflectedTypeId => Dy.Order;

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

    public DyComparableTypeInfo()
    {
        Closed = true;
        AddDefaultMixin2(Builtins.Gt, "other");
        AddDefaultMixin2(Builtins.Lt, "other");
        AddDefaultMixin2(Builtins.Gte, "other");
        AddDefaultMixin2(Builtins.Lte, "other");
    }
}
