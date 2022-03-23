using System.Globalization;

namespace Dyalect.Library
{
    internal static class CI
    {
        public static readonly CultureInfo Default = new("en-US");

        public static readonly NumberFormatInfo NumberFormat = Default.NumberFormat;
    }
}
