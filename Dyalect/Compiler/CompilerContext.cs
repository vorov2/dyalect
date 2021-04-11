using Dyalect.Parser.Model;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    internal sealed class CompilerContext
    {
        public CompilerContext()
        {
            FunctionAddress = -1;
            BlockExit = Label.Empty;
            BlockSkip = Label.Empty;
            BlockBreakExit = Label.Empty;
            FunctionStart = Label.Empty;
            FunctionExit = Label.Empty;
            MatchExit = Label.Empty;
        }

        public CompilerContext(CompilerContext old)
        {
            FunctionAddress = old.FunctionAddress;
            Function = old.Function;
            FunctionStart = old.FunctionStart;
            FunctionExit = old.FunctionExit;
            BlockBreakExit = old.BlockBreakExit;
            BlockExit = old.BlockExit;
            BlockSkip = old.BlockSkip;
            MatchExit = old.MatchExit;
        }

        public Stack<int> Errors { get; } = new();

        public DFunctionDeclaration Function { get; set; }

        public int FunctionAddress { get; set; }

        public Label FunctionStart { get; set; }

        public Label FunctionExit { get; set; }

        public Label BlockBreakExit { get; set; }

        public Label BlockExit { get; set; }

        public Label BlockSkip { get; set; }

        public Label MatchExit { get; set; }
    }
}