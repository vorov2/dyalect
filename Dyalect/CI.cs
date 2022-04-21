using System.Globalization;

namespace Dyalect
{
    public static class CI
    {
        public static readonly CultureInfo Default = new("en-US");

        public static readonly CultureInfo UI = Default;

        public static readonly NumberFormatInfo NumberFormat = Default.NumberFormat;
    }
}
