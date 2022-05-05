using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal sealed class DyMissingMethod : DyForeignFunction
{
    private static readonly Par[] parameters = new Par[] { new Par("%args", ParKind.VarArg) };

    internal const string Name = "MissingMethod";
    private readonly DyNativeFunction fun;
    private readonly string missingMethodName;

    public DyMissingMethod(string name, DyNativeFunction fun) : base(Name, parameters, 0) => 
        (this.fun, missingMethodName) = (fun, name);

    protected override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args)
    {
        var fn = Self is not null ? fun.BindToInstance(ctx, Self) : fun;
        var pars = new DyObject[2];
        pars[0] = new DyString(missingMethodName);
        pars[1] = args.Length == 0 ? DyTuple.Empty : (DyTuple)args[0];
        return fn.Call(ctx, pars);
    }

    protected override DyFunction Clone(ExecutionContext ctx) => new DyMissingMethod(missingMethodName, fun);

    public override object ToObject() => fun.ToObject();

    protected override bool Equals(DyFunction func) =>
           func is DyMissingMethod mi && mi.fun.Equals(fun)
        && IsSameInstance(this, func);
}
