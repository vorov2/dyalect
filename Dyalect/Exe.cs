using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect
{
    public static class Exe
    {
        public static DyObject Eval(string sourceCode, DyTuple args = null, BuilderOptions options = null)
        {
            var linker = new DyLinker(null, options ?? BuilderOptions.Default, args);
            var result = linker.Make(SourceBuffer.FromString(sourceCode));

            if (!result.Success)
                throw new DyBuildException(result.Messages);

            var ctx = DyMachine.CreateExecutionContext(result.Value);
            var result2 = DyMachine.Execute(ctx);
            return result2.Value;
        }
    }

}