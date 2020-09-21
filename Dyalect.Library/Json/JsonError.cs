namespace Dyalect.Library.Json
{
    public sealed class JsonError
    {
        internal JsonError(string message, Location loc)
        {
            Message = message;
            Location = loc;
        }

        public string Message { get; }

        public Location Location { get; }
    }
}