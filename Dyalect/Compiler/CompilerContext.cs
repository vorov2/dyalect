namespace Dyalect.Compiler
{
    internal sealed class CompilerContext
    {
        public CompilerContext()
        {
            BlockExit = Label.Empty;
            BlockSkip = Label.Empty;
            FunctionExit = Label.Empty;
        }

        public CompilerContext(CompilerContext old)
        {
            FunctionExit = old.FunctionExit;
            BlockBreakExit = Label.Empty;
            BlockExit = Label.Empty;
            BlockSkip = Label.Empty;
        }

        public ContextKind Kind { get; set; }

        public Label FunctionExit { get; set; }

        public Label BlockBreakExit { get; set; }

        public Label BlockExit { get; set; }

        public Label BlockSkip { get; set; }
    }

    internal enum ContextKind
    {
        Global = 0,
        Function,
        MemberFunction
    }
}