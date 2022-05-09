using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyComparableTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Order);

    public override int ReflectedTypeId => Dy.Order;

    public DyComparableTypeInfo()
    {
        Closed = true;
        AddDefaultMixin(Builtins.Gt, "other");
        AddDefaultMixin(Builtins.Lt, "other");
        AddDefaultMixin(Builtins.Gte, "other");
        AddDefaultMixin(Builtins.Lte, "other");
    }
}
