namespace Dyalect.Library.Json
{
    internal static class BufferExtensions
    {
        public static char Lookup(this char[] buffer, int pos)
        {
            if (pos < buffer.Length && pos >= 0)
                return buffer[pos];
            else
                return '\0';
        }
    }
}