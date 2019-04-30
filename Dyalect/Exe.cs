using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect
{
    public static class Exe
    {
        public static DyObject Eval(SourceBuffer buffer, BuilderOptions options = null)
        {
            var linker = new DyLinker(FileLookup.Create(buffer.FileName), options ?? BuilderOptions.Default);
            var result = linker.Make(buffer);

            if (!result.Success)
                throw new DyBuildException(result.Messages);

            var ctx = DyMachine.CreateExecutionContext(result.Value);
            var result2 = DyMachine.Execute(ctx);
            return result2.Value;
        }
    }

}