namespace Dyalect.Debug
{
    public sealed class LineSym
    {
        public LineSym() { }

        public LineSym(int offset) => Offset = offset;

        public LineSym(int offset, int line, int column) => 
            (Offset, Line, Column) = (offset, line, column);

        public int Offset { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }
    }
}
