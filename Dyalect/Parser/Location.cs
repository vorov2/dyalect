namespace Dyalect.Parser
{
    public struct Location
    {
        public Location(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public readonly int Line;
        public readonly int Column;
        public bool IsEmpty => Line == 0 && Column == 0;
    }
}
