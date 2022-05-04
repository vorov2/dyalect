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

    protected override DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args)
    {
        var res = first.Call(ctx, args);

        if (ctx.HasErrors)
            return Nil;

        return second.Call(ctx, res);
    }

    protected override bool Equals(DyFunction func) => 
           func is CompositionContainer cc
        && cc.first.Equals(first) && cc.second.Equals(second);
}
