using Dyalect.Compiler;
using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Linker
{
    public sealed class DyIncrementalLinker : DyLinker
    {
        private DyCompiler compiler;
        private UnitComposition composition;
        private int? startOffset;

        public DyIncrementalLinker(FileLookup lookup, BuilderOptions options) : base(lookup, options)
        {

        }

        protected override Result<UnitComposition> Make(Unit unit)
        {
            Messages.Clear();

            if (composition == null)
                composition = new UnitComposition(Units);

            Units[0] = unit;
            ProcessUnits(composition);
            return Result.Create(composition, Messages);
        }

        protected override Unit CompileNodes(DyCodeModel codeModel, bool root)
        {
            if (!root)
                return base.CompileNodes(codeModel, root);

            var oldCompiler = compiler;

            if (compiler == null)
                compiler = new DyCompiler(Options, this);
            else
            {
                compiler = new DyCompiler(compiler);
                startOffset = composition.Units[0].Ops.Count;
            }

            var res = compiler.Compile(codeModel);

            if (!res.Success)
            {
                compiler = oldCompiler;
                startOffset = null;
                Messages.AddRange(res.Messages);
                return null;
            }

            if (startOffset != null)
                res.Value.Layouts[0].Address = startOffset.Value;

            return res.Value;
        }
    }
}
