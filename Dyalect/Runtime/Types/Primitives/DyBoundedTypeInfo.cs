using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyBoundedTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => DyTypeNames.Bounded;

    public override int ReflectedTypeId => DyType.Bounded;

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

    public DyBoundedTypeInfo()
    {
        Closed = true;
        AddDefaultMixin1(Builtins.Max);
        AddDefaultMixin1(Builtins.Min);
    }
}
