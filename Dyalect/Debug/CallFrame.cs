namespace Dyalect.Debug;

public class CallFrame
{
    private const string Format = "\tat {0} in {1}, line {2}, column {3}";
    private const string ShortFormat = "\tat {0} in {1}, offset {2}";
    private const string ExternalPoint = "\tat <external code>";
    private const string Global = "<global>";

    internal static readonly CallFrame External = new ExternalCallFrame();

    private sealed class ExternalCallFrame : CallFrame
    {
        internal ExternalCallFrame() : base("", "", 0, LineSym.Empty) { }

        public override string ToString() => ExternalPoint;
    }

    private string GetName() => CodeBlockName ?? Global;

    public string? CodeBlockName { get; }

    public string? ModuleName { get; }

    public int Offset { get; }

    public LineSym? LinePragma { get; }

    internal CallFrame(string? moduleName, string codeBlockName, int offset, LineSym lineSym) =>
        (CodeBlockName, ModuleName, Offset, LinePragma) = (moduleName, codeBlockName, offset, lineSym);

    public override string ToString() =>
        LinePragma != null
            ? string.Format(Format, GetName(), ModuleName, LinePragma.Line, LinePragma.Column)
            : string.Format(ShortFormat, GetName(), ModuleName, Offset);
}
