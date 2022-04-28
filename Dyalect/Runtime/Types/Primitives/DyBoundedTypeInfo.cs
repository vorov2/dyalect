using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyBoundedTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Bounded);

    public override int ReflectedTypeId => Dy.Bounded;

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

    public DyBoundedTypeInfo()
    {
        Closed = true;
        AddDefaultMixin1(Builtins.Max);
        AddDefaultMixin1(Builtins.Min);
    }
}
