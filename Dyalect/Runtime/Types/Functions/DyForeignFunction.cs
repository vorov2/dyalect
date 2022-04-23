using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

public abstract class DyForeignFunction : DyFunction
{
    private readonly string name;

    protected DyForeignFunction(string? name, Par[] pars, int varArgIndex)
        : base(pars, varArgIndex) => this.name = name ?? DefaultName;

    public override string FunctionName => name;

    public override bool IsExternal => true;

    internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg)
    {
        var clone = Clone(ctx);
        clone.Self = arg;
        return clone;
    }

    protected virtual DyFunction Clone(ExecutionContext ctx) => (DyForeignFunction)MemberwiseClone();

    internal override DyObject[] CreateLocals(ExecutionContext ctx) =>
        Parameters.Length == 0 ? Array.Empty<DyObject>() : new DyObject[Parameters.Length];

    internal override DyObject InternalCall(ExecutionContext ctx) => InternalCall(ctx, Array.Empty<DyObject>());
}
