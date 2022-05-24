namespace Dyalect.Parser;

public record struct Location(int Line, int Column)
{
    public bool IsEmpty => Line == 0 && Column == 0;
}
