using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyFunctorTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Functor);

    public override int ReflectedTypeId => Dy.Functor;

    public DyFunctorTypeInfo()
    {
        Members.Add(Builtins.Call, Unary(Builtins.Call, SelfCall));
    }

    private static DyObject SelfCall(ExecutionContext ctx, DyObject self) =>
        ctx.NotImplemented(Builtins.Call);
}
