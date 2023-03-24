using System;

namespace Dyalect.Library.Core;

internal static class DT
{
    public const long TicksPerDay = 24 * TicksPerHour;
    public const long TicksPerHour = 60 * TicksPerMinute;
    public const long TicksPerMinute = 60 * TicksPerSecond;
    public const long TicksPerSecond = 10 * TicksPerDecisecond;
    public const long TicksPerDecisecond = 10 * TicksPerCentisecond;
    public const long TicksPerCentisecond = 10 * TicksPerMillisecond;
    public const long TicksPerMillisecond = 1000 * TicksPerMicrosecond;
    public const long TicksPerMicrosecond = 10L;

    public static long Sum(long days, long hours, long minutes, long sec, long ms) =>
        days * TimeSpan.TicksPerDay + hours * TimeSpan.TicksPerHour + minutes * TimeSpan.TicksPerMinute
        + sec * TimeSpan.TicksPerSecond + ms * TimeSpan.TicksPerMillisecond;
}
