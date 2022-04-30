using Dyalect.Compiler;
using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

public abstract class DyForeignFunction : DyFunction
{
    public override string FunctionName { get; }

    public override bool IsExternal => true;

    protected DyForeignFunction(string? name, Par[] pars, int varArgIndex)
        : base(pars, varArgIndex) => FunctionName = name ?? DefaultName;

    protected DyForeignFunction(string? name, Par[] pars) : this(name, pars, GetVarArgIndex(pars)) { }

    private static int GetVarArgIndex(Par[] pars)
    {
        for (var i = 0; i < pars.Length; i++)
            if (pars[i].IsVarArg)
                return i;

        return -1;
    }

    internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg)
    {
        var clone = Clone(ctx);
        clone.Self = arg;
        return clone;
    }

    protected virtual DyFunction Clone(ExecutionContext ctx) => (DyForeignFunction)MemberwiseClone();

    internal override DyObject[] CreateLocals(ExecutionContext ctx) =>
        Parameters.Length == 0 ? Array.Empty<DyObject>() : new DyObject[Parameters.Length];

    internal sealed override MemoryLayout? GetLayout(ExecutionContext ctx) => throw new NotSupportedException();
}
