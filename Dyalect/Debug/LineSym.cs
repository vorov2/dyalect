namespace Dyalect.Debug
{
    public sealed class LineSym
    {
        public LineSym(int offset, int line, int column)
        {
            Offset = offset;
            Line = line;
            Column = column;
        }

        public LineSym()
        {

        }

        public int Offset { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }
    }
}
