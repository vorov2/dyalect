namespace Dyalect.Debug;

public sealed class ScopeSym
{
    public int Index { get; init; }

    public int ParentIndex { get; init; }

    public int StartOffset { get; init; }

    public int EndOffset { get; internal set; }

    public int StartLine { get; init; }

    public int StartColumn { get; init; }

    public int EndLine { get; internal set; }

    public int EndColumn { get; internal set; }

    public ScopeSym() { }

    public ScopeSym(int index, int parentIndex, int startOffset, int endOffset, int startLine,
        int startColumn, int endLine, int endColumn)
    {
        Index = index;
        ParentIndex = parentIndex;
        StartOffset = startOffset;
        EndOffset = endOffset;
        StartLine = startLine;
        StartColumn = startColumn;
        EndLine = endLine;
        EndColumn = endColumn;
    }
}
