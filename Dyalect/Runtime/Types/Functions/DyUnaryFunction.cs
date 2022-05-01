using Dyalect.Compiler;
using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal class DyUnaryFunction : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject, DyObject> fun;

    public DyUnaryFunction(string name, Func<ExecutionContext, DyObject, DyObject> fun)
        : base(name, Array.Empty<Par>(), -1) => this.fun = fun;

    public DyUnaryFunction(string name, Func<ExecutionContext, DyObject, DyObject> fun, bool isPropertyGetter)
        : this(name, fun)
    {
        if (isPropertyGetter)
            Attr |= FunAttr.Auto;
    }

    internal override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!);

    internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg)
    {
        if (!Auto)
            return BindToInstance(ctx, arg);

        return fun(ctx, arg);
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new DyUnaryFunction(FunctionName, fun);

    public override object ToObject() => fun;

    internal override bool Equals(DyFunction func) => 
           func is DyUnaryFunction bin && ReferenceEquals(bin.fun, fun)
        && IsSameInstance(this, func);
}