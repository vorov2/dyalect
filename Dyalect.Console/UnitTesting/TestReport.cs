using System.Collections.Generic;

namespace Dyalect.UnitTesting;

public sealed class TestReport
{
    public string[] TestFiles { get; init; } = null!;

    public List<BuildMessage>? BuildWarnings { get; set; }

    public List<TestResult> Results { get; } = new();

    public List<string> FailedFiles { get; } = new();
}
