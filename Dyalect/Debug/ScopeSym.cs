namespace Dyalect.Debug
{
    public sealed class ScopeSym
    {
        public ScopeSym() { }

        public ScopeSym(int index, int parentIndex, int startOffset, int startLine, int startColumn)
        {
            Index = index;
            ParentIndex = parentIndex;
            StartOffset = startOffset;
            StartLine = startLine;
            StartColumn = startColumn;
        }

        public int Index { get; set; }

        public int ParentIndex { get; set; }

        public int StartOffset { get; set; }

        public int EndOffset { get; set; }

        public int StartLine { get; set; }

        public int StartColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }
    }
}
