namespace Dyalect.UnitTesting;

public sealed class TestResult
{
    public string Name { get; init; } = null!;

    public string FileName { get; init; } = null!;

    public string? Error { get; init; }
}
