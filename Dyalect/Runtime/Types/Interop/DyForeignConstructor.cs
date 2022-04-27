using Dyalect.Debug;
using System;
namespace Dyalect.Runtime.Types;

internal sealed class DyForeignConstructor : DyForeignFunction
{
    private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

    public DyForeignConstructor(Func<ExecutionContext, DyObject, DyObject, DyObject> fun)
        : base("new", new Par[] { new Par("values", ParKind.VarArg) }, 0) => this.fun = fun;

    internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0]);

    protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction1(FunctionName, fun, Parameters, VarArgIndex);
}
