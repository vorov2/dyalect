using Dyalect.Runtime.Types;
namespace Dyalect.Runtime;

internal sealed class Caller
{
    public static readonly Caller Root = new();
    public static readonly Caller External = new();

    public readonly DyObject[] Locals;
    public readonly EvalStack EvalStack;
    public readonly int Offset;
    public readonly DyNativeFunction Function;

    private Caller()
    {
        Locals = Array.Empty<DyObject>();
        EvalStack = new(0);
        Function = new(null, 0, 0, FastList<DyObject[]>.Empty, 0);
    }

    public Caller(DyNativeFunction function, int offset, EvalStack evalStack, DyObject[] locals) =>
        (Function, Offset, EvalStack, Locals) = (function, offset, evalStack, locals);
}
