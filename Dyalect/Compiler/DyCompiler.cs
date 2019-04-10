using Dyalect.Linker;
using Dyalect.Parser.Model;

namespace Dyalect.Compiler
{
    public sealed class DyCompiler
    {
        private readonly BuilderOptions options;
        private readonly DyLinker linker;
        private readonly Builder builder;

        public DyCompiler(BuilderOptions options, DyLinker linker)
        {
            this.options = options ?? BuilderOptions.Default;
            this.linker = linker;
            this.builder = new Builder(this.options, linker);
        }

        public DyCompiler(DyCompiler compiler)
        {
            this.options = compiler.options;
            this.linker = compiler.linker;
            this.builder = new Builder(compiler.builder);
        }

        public Result<Unit> Compile(DyCodeModel codeModel)
        {
            var unit = builder.Build(codeModel);
            return Result.Create(unit, builder.Messages);
        }
    }
}
