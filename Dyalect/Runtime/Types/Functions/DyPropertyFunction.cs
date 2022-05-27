using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal class DyPropertyFunction : DyForeignFunction
{
    private readonly static Par[] pars = new Par[] { new ("value", DyNil.Terminator) };
    private readonly DyFunction getter;
    private readonly DyFunction setter;

    public DyPropertyFunction(string name, DyFunction getter, DyFunction setter)
        : base(name, pars, -1) => (this.getter, this.setter) = (getter, setter);

    protected override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args)
    {
        if (ReferenceEquals(args[0], DyNil.Terminator))
            return getter.Call(ctx);
        else
            return setter.Call(ctx, args);
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new DyPropertyFunction(FunctionName, getter, setter);

    public override object ToObject() => getter.ToObject();

    protected override bool Equals(DyFunction func) =>
           func is DyPropertyFunction other
        && other.getter.Equals(getter)
        && other.setter.Equals(setter)
        && IsSameInstance(this, func);
}