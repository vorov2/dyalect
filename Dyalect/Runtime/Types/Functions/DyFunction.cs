using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Parser;
using System.Runtime.CompilerServices;
using System.Text;
namespace Dyalect.Runtime.Types;

public abstract class DyFunction : DyObject
{
    internal const string DefaultName = "<func>";
    internal protected DyObject? Self;
    internal Par[] Parameters;
    internal protected int VarArgIndex;
    internal protected int Attr;

    public override string TypeName => nameof(Dy.Function);

    public abstract string FunctionName { get; }

    public abstract bool IsExternal { get; }

    internal bool Auto => (Attr & FunAttr.Auto) == FunAttr.Auto;

    internal bool Final => (Attr & FunAttr.Final) == FunAttr.Final;

    internal bool VariantConstructor => (Attr & FunAttr.Variadic) == FunAttr.Variadic;

    protected DyFunction(Par[] pars, int varArgIndex) : base(Dy.Function) =>
        (Parameters, VarArgIndex) = (pars, varArgIndex);

    public override object ToObject() => (Func<ExecutionContext, DyObject[], DyObject>)Call;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal DyObject PrepareFunction(ExecutionContext ctx, DyObject self)
    {
        if (IsExternal)
            return ((DyUnaryFunction)this).CallUnary(ctx, self);

        var func = BindToInstance(ctx, self);
        ctx.EtaFunction = func;
        ctx.Error = DyVariant.Eta;
        return Nil;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal DyObject PrepareFunction(ExecutionContext ctx, DyObject self, DyObject arg)
    {
        if (IsExternal)
            return ((DyBinaryFunction)this).CallBinary(ctx, self, arg);

        var func = BindToInstance(ctx, self);
        ctx.EtaFunction = func;
        ctx.Error = DyVariant.Eta;
        return Nil;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal DyObject PrepareFunction(ExecutionContext ctx, DyObject self, DyObject arg1, DyObject arg2)
    {
        if (IsExternal)
            return ((DyTernaryFunction)this).CallTernary(ctx, self, arg1, arg2);

        var func = BindToInstance(ctx, self);
        ctx.EtaFunction = func;
        ctx.Error = DyVariant.Eta;
        return Nil;
    }

    internal abstract DyFunction BindToInstance(ExecutionContext ctx, DyObject arg);

    internal DyObject TryInvokeProperty(ExecutionContext ctx, DyObject arg) => BindOrRun(ctx, arg);

    protected virtual DyObject BindOrRun(ExecutionContext ctx, DyObject arg) => BindToInstance(ctx, arg);

    internal DyObject FastCall(ExecutionContext ctx, DyObject[] args) => CallWithMemoryLayout(ctx, args);

    protected abstract DyObject CallWithMemoryLayout(ExecutionContext ctx, DyObject[] args);

    public DyObject Call(ExecutionContext ctx)
    {
        var newArgs = PrepareMemoryLayout(ctx, Array.Empty<DyObject>());

        if (ctx.HasErrors)
            return Nil;

        return CallWithMemoryLayout(ctx, newArgs);
    }

    public DyObject Call(ExecutionContext ctx, params DyObject[] args)
    {
        var newArgs = PrepareMemoryLayout(ctx, args);

        if (ctx.HasErrors)
            return Nil;

        return CallWithMemoryLayout(ctx, newArgs);
    }

    protected DyObject[] PrepareMemoryLayout(ExecutionContext ctx, DyObject[] args)
    {
        if (args.Length > Parameters.Length)
        {
            ctx.TooManyArguments(FunctionName, Parameters.Length, args.Length);
            return args;
        }

        DyObject[] newLocals;
        var needDefaults = false;
        var memorySize = GetMemoryCells(ctx);

        if (args.Length == memorySize)
            newLocals = args;
        else
        {
            needDefaults = true;
            newLocals = new DyObject[memorySize];
            if (args.Length > 0)
                Array.Copy(args, newLocals, args.Length);
        }

        if (VarArgIndex > -1)
        {
            var o = newLocals[VarArgIndex];
            if (o.TypeId == Dy.Nil)
                newLocals[VarArgIndex] = DyTuple.Empty;
            else if (o.TypeId == Dy.Array)
            {
                var arr = (DyArray)o;
                arr.Compact();
                newLocals[VarArgIndex] = new DyTuple(arr.UnsafeAccess(), arr.Count);
            }
            else if (o.TypeId != Dy.Tuple)
                newLocals[VarArgIndex] = new DyTuple(new[] { o });
        }

        if (needDefaults)
            DyMachine.FillDefaults(newLocals, this, ctx);

        return newLocals;
    }

    internal int GetParameterIndex(string name)
    {
        for (var i = 0; i < Parameters.Length; i++)
        {
            if (Parameters[i].Name == name)
                return i;
        }

        return -1;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (FunctionName is null)
            sb.Append(DefaultName);
        else
            sb.Append(FunctionName);

        sb.Append('(');
        var c = 0;

        foreach (var p in Parameters)
        {
            if (c != 0)
                sb.Append(", ");

            sb.Append(p.Name);

            if (p.IsVarArg)
                sb.Append("...");

            if (p.Value is not null)
            {
                sb.Append(" = ");
                if (p.Value is DyString)
                    sb.Append(StringUtil.Escape(p.Value.ToString()!));
                else if (p.Value is DyChar)
                    sb.Append(StringUtil.Escape(p.Value.ToString()!, "'"));
                else
                    sb.Append(p.Value.ToString());
            }

            c++;
        }

        sb.Append(')');
        var ret = sb.ToString();
        return ret;
    }

    //Checks if two functions are members of the same instance
    public static bool IsSameInstance(DyFunction first, DyFunction second) =>
        ReferenceEquals(first.Self, second.Self) || (first.Self is not null && first.Self.Equals(second.Self));

    internal abstract int GetMemoryCells(ExecutionContext ctx);

    internal abstract DyObject[] CreateLocals(ExecutionContext ctx);

    protected abstract bool Equals(DyFunction func);

    public sealed override bool Equals(DyObject? other) => other is DyFunction func && Equals(func);

    public override int GetHashCode() => HashCode.Combine(TypeId, FunctionName ?? DefaultName, Parameters, Self);
}
