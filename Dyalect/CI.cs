using System.Globalization;

namespace Dyalect
{
    internal static class CI
    {
        public static readonly CultureInfo Default = new CultureInfo("en-US");

        public static readonly NumberFormatInfo NumberFormat = Default.NumberFormat;
    }
}
