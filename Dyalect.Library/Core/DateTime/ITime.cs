namespace Dyalect.Library.Core;

public interface ITime : ISpan
{
    int Hours { get; }

    int Minutes { get; }

    int Seconds { get; }

    int Milliseconds { get; }

    int Microseconds { get; }

    int Ticks { get; }
}
