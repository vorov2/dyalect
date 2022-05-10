using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyFunctorTypeInfo : DyMixin<DyFunctorTypeInfo>
{
    public DyFunctorTypeInfo() : base(Dy.Functor)
    {
        Members.Add(Builtins.Call, Unary(Builtins.Call, SelfCall));
    }

    private static DyObject SelfCall(ExecutionContext ctx, DyObject self) =>
        ctx.NotImplemented(Builtins.Call);
}
