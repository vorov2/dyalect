using System;

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

        public Label FunctionExit { get; set; }

        public Label BlockBreakExit { get; set; }

        public Label BlockExit { get; set; }

        public Label BlockSkip { get; set; }
    }
}