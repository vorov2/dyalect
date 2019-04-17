using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public sealed class ExecutionResult
    {
        private ExecutionResult(long ticks, DyObject value, ExecutionContext ctx, TerminationReason reason)
        {
            Ticks = ticks;
            Value = value;
            Context = ctx;
        }

        internal static ExecutionResult Fetch(long ticks, DyObject value, ExecutionContext ctx)
        {
            return new ExecutionResult(ticks, value, ctx, TerminationReason.Complete);
        }

        internal static ExecutionResult Abort(long ticks, ExecutionContext ctx)
        {
            return new ExecutionResult(ticks, null, ctx, TerminationReason.Abort);
        }

        public long Ticks { get; private set; }

        public DyObject Value { get; private set; }

        public ExecutionContext Context { get; private set; }

        public TerminationReason Reason { get; private set; }
    }
}
