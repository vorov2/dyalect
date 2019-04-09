using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public sealed class ExecutionResult
    {
        private ExecutionResult(long ticks, DyObject value, TerminationReason reason)
        {
            Ticks = ticks;
            Value = value;
        }

        internal static ExecutionResult Fetch(long ticks, DyObject value)
        {
            return new ExecutionResult(ticks, value, TerminationReason.Complete);
        }

        internal static ExecutionResult Abort(long ticks)
        {
            return new ExecutionResult(ticks, null, TerminationReason.Abort);
        }

        public long Ticks { get; private set; }

        public DyObject Value { get; private set; }

        public TerminationReason Reason { get; private set; }
    }
}
