using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyIdentityTypeInfo : DyMixin<DyIdentityTypeInfo>
{
    public DyIdentityTypeInfo() : base(Dy.Identity)
    {
        Members.Add(Builtins.Clone, Unary(Builtins.Clone, GetIdentity));
    }

    private static DyObject GetIdentity(ExecutionContext _, DyObject arg) => arg;
}
