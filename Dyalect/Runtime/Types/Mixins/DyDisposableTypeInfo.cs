using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyDisposableTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Disposable);

    public override int ReflectedTypeId => Dy.Disposable;

    public DyDisposableTypeInfo()
    {
        Members.Add(Builtins.Dispose, Unary(Builtins.Dispose, Dispose));
    }

    private static DyObject Dispose(ExecutionContext ctx, DyObject self) =>
        ctx.NotImplemented(Builtins.Dispose);
}
