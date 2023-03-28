using Dyalect.Compiler;

namespace Dyalect.Runtime.Types;

internal sealed class DyIdentityMixin : DyMixin<DyIdentityMixin>
{
    public DyIdentityMixin() : base(Dy.Identity) =>
        Members.Add(Builtins.Clone, Unary(Builtins.Clone, GetIdentity));

    private static DyObject GetIdentity(ExecutionContext _, DyObject arg) => arg;
}
