namespace Dyalect.Runtime.Types;

internal sealed class CompositionContainer : DyForeignFunction
{
    private readonly DyFunction first;
    private readonly DyFunction second;

    public CompositionContainer(DyFunction first, DyFunction second) : base(null, first.Parameters, first.VarArgIndex)
    {
        this.first = first;
        this.second = second;
    }

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
    {
        var res = first.Call(ctx, args);

        if (ctx.HasErrors)
            return Nil;

        return second.Call(ctx, res);
    }

    internal override bool Equals(DyFunction func) => 
           func is CompositionContainer cc
        && cc.first.Equals(first) && cc.second.Equals(second);
}
