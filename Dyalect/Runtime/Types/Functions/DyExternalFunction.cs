using Dyalect.Compiler;
using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

public sealed class DyExternalFunction : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject?, DyObject[], DyObject> func;

    public DyExternalFunction(string name, bool isPropertyGetter, Func<ExecutionContext, DyObject?, DyObject[], DyObject> func, params Par[] pars)
        : base(name, pars)
    {
        this.func = func;

        if (isPropertyGetter)
            Attr |= FunAttr.Auto;
    }

    public override DyObject Clone() => new DyExternalFunction(FunctionName, (Attr & FunAttr.Auto) == FunAttr.Auto, func, Parameters);

    internal override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args) =>
        func(ctx, Self, args);

    internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg)
    {
        if (!Auto)
            return BindToInstance(ctx, arg);

        return func(ctx, arg, Array.Empty<DyObject>());
    }

    public override object ToObject() => func;

    protected override bool Equals(DyFunction func) =>
           FunctionName == func.FunctionName
        && func is DyExternalFunction fn && fn.func.Equals(func)
        && IsSameInstance(this, func);
}
