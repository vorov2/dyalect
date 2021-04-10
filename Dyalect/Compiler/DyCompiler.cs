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
            this.options = options ?? BuilderOptions.Default();
            this.linker = linker;
            builder = new(this.options, linker);
        }

        public DyCompiler(DyCompiler compiler)
        {
            options = compiler.options;
            linker = compiler.linker;
            builder = new(compiler.builder);
        }

        public Result<Unit> Compile(DyCodeModel codeModel)
        {
            var unit = builder.Build(codeModel);
            return Result.Create(unit, builder.Messages);
        }
    }
}
