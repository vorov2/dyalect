using Dyalect.Debug;
using System;
using System.Runtime.CompilerServices;

namespace Dyalect.Runtime.Types;

public abstract class DyForeignFunction : DyFunction
{
    private readonly string name;

    protected DyForeignFunction(string? name, Par[] pars, int varArgIndex)
        : base(pars, varArgIndex) => this.name = name ?? DefaultName;

    protected DyForeignFunction(string? name, Par[] pars) : base(pars, -1)
    {
        this.name = name ?? DefaultName;

        for (var i = 0; i < pars.Length; i++)
            if (pars[i].IsVarArg)
            {
                VarArgIndex = i;
                break;
            }
    }

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

    internal override bool Equals(DyFunction func) => ReferenceEquals(this, func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected T? Cast<T>(int index, DyObject[] args) where T : DyObject
    {
        try
        {
            if (args[index].IsNil() && Parameters[index].Value.IsNil())
                return default;

            return (T)args[index];
        }
        catch (InvalidCastException)
        {
            throw new DyErrorException(new(DyErrorCode.InvalidType, args[index].TypeName));
        }
    }
}
