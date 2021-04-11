using System.Collections.Generic;
using System.Linq;

namespace Dyalect
{
    public abstract class Result
    {
        protected Result(IEnumerable<BuildMessage> messages) =>
            (Messages, Success) = (messages, !messages.Any(m => m.Type == BuildMessageType.Error));

        public static Result<T> Create<T>(T result, IEnumerable<BuildMessage> messages = null) =>
            new(result, messages);

        public IEnumerable<BuildMessage> Messages { get; }

        public bool Success { get; }
    }

    public sealed class Result<T> : Result
    {
        internal Result(T result, IEnumerable<BuildMessage> messages = null)
            : base(messages ?? Enumerable.Empty<BuildMessage>()) => Value = result;

        public T Value { get; }
    }
}
