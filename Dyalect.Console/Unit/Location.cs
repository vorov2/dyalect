namespace Dyalect.Units
{
    public struct Location
    {
        public readonly int Line;
        public readonly int Column;

        public Location(int line, int column) => (Line, Column) = (line, column);

        public bool IsEmpty => Line == 0 && Column == 0;
    }
}
