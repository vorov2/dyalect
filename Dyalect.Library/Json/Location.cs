namespace Dyalect.Library.Json
{
    public struct Location
    {
        public Location(int line, int col)
        {
            Line = line;
            Col = col;
        }

        public readonly int Line;
        public readonly int Col;

        public override string ToString()
        {
            return $"({Line},{Col})";
        }
    }
}