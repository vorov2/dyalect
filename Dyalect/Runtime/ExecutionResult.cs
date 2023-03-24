using Dyalect.Runtime.Types;

namespace Dyalect.Runtime;

public sealed class ExecutionResult
{
    public long Ticks { get; }

    public DyObject? Value { get; }

    public ExecutionContext Context { get; }

    public TerminationReason Reason { get; }

    private ExecutionResult(long ticks, DyObject? value, ExecutionContext ctx, TerminationReason reason) =>
        (Ticks, Value, Context, Reason) = (ticks, value, ctx, reason);

    internal static ExecutionResult Fetch(long ticks, DyObject? value, ExecutionContext ctx) =>
        new(ticks, value, ctx, TerminationReason.Complete);

    internal static ExecutionResult Abort(long ticks, ExecutionContext ctx) =>
        new(ticks, null, ctx, TerminationReason.Abort);
}
