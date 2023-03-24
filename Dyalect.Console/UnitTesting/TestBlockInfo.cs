using Dyalect.Parser.Model;

namespace Dyalect.UnitTesting;

public sealed class TestBlockInfo
{
    public DRegion? Block { get; init; }

    public string? Error { get; init; }

    public string FileName { get; }

    public TestBlockInfo(string fileName) => FileName = fileName;
}
