using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyDisposableTypeInfo : DyMixin<DyDisposableTypeInfo>
{
    public DyDisposableTypeInfo() : base(Dy.Disposable)
    {
        Members.Add(Builtins.Dispose, Unary(Builtins.Dispose, Dispose));
    }

    private static DyObject Dispose(ExecutionContext ctx, DyObject self) =>
        ctx.NotImplemented(Builtins.Dispose);
}
