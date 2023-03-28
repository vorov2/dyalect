using Dyalect.Compiler;

namespace Dyalect.Runtime.Types;

internal sealed class DyFunctorMixin : DyMixin<DyFunctorMixin>
{
    public DyFunctorMixin() : base(Dy.Functor) =>
        Members.Add(Builtins.Call, Unary(Builtins.Call, SelfCall));

    private static DyObject SelfCall(ExecutionContext ctx, DyObject self) =>
        ctx.NotImplemented(Builtins.Call);
}
