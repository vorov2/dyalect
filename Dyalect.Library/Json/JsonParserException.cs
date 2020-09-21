using System;

namespace Dyalect.Library.Json
{
    public sealed class JsonParserException : Exception
    {
        public JsonParserException(string message, int line, int col)
            : base($"{message} ({line},{col}).")
        {
            Line = line;
            Col = col;
        }

        public int Line { get; }

        public int Col { get; }
    }
}