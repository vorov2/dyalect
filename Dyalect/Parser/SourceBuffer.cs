using System.IO;

namespace Dyalect.Parser
{
    public abstract class SourceBuffer
    {
        internal const int EOF = char.MaxValue + 1;

        public abstract string FileName { get; }

        internal protected abstract int Pos { get; set; }

        internal protected abstract string GetString(int start, int end);

        internal protected abstract int Peek();

        internal protected abstract int Read();

        public static SourceBuffer FromFile(string file)
        {
            using (var sr = new StreamReader(File.OpenRead(file)))
                return new StringBuffer(sr.ReadToEnd(), file);
        }

        public static SourceBuffer FromString(string str, string file = null)
        {
            return new StringBuffer(str, file ?? "<memory>");
        }
    }
}