using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyIdentityTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Identity);

    public override int ReflectedTypeId => Dy.Identity;

    public DyIdentityTypeInfo()
    {
        Members.Add(Builtins.Clone, Unary(Builtins.Clone, GetIdentity));
    }

    private static DyObject GetIdentity(ExecutionContext _, DyObject arg) => arg;
}
