namespace Dyalect.Library.Core
{
    internal static class DateHelper
    {
        public const long TicksPerDay = 24 * TicksPerHour;
        public const long TicksPerHour = 60 * TicksPerMinute;
        public const long TicksPerMinute = 60 * TicksPerSecond;
        public const long TicksPerSecond = 10 * TicksPerDecisecond;
        public const long TicksPerDecisecond = 10 * TicksPerCentisecond;
        public const long TicksPerCentisecond = 10 * TicksPerMillisecond;
        public const long TicksPerMillisecond = 1000 * TicksPerMicrosecond;
        public const long TicksPerMicrosecond = 10L;
    }
}
