using Dyalect.Compiler;

namespace Dyalect.Runtime.Types;

internal sealed class DyDisposableMixin : DyMixin<DyDisposableMixin>
{
    public DyDisposableMixin() : base(Dy.Disposable)
    {
        Members.Add(Builtins.Dispose, Unary(Builtins.Dispose, Dispose));
    }

    private static DyObject Dispose(ExecutionContext ctx, DyObject self) =>
        ctx.NotImplemented(Builtins.Dispose);
}
