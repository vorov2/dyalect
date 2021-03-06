﻿using Dyalect.Compiler;
using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Linker
{
    public sealed class DyIncrementalLinker : DyLinker
    {
        private DyCompiler? compiler;
        private UnitComposition? composition;
        private int? startOffset;

        private Dictionary<Reference, Unit>? backupUnitMap;
        private Dictionary<string, Dictionary<string, Type>>? backupAssemblyMap;
        private List<Unit>? backupUnits;

        public DyIncrementalLinker(FileLookup lookup, BuilderOptions options) : base(lookup, options) { }

        public DyIncrementalLinker(FileLookup lookup, BuilderOptions options, DyTuple? args) : base(lookup, options, args) { }

        protected override void Prepare()
        {
            backupUnitMap = new(UnitMap);
            backupAssemblyMap = new(AssemblyMap);
            backupUnits = new(Units!);
        }

        protected override void Complete(bool failed)
        {
            if (!failed)
                return;
            
            if (backupUnitMap is not null)
                UnitMap = backupUnitMap;

            if (backupAssemblyMap is not null)
                AssemblyMap = backupAssemblyMap;

            Units.Clear();

            if (backupUnits is null)
                return;
            
            for (var i = 0; i < backupUnits.Count; i++)
                Units.Add(backupUnits[i]);
        }

        protected override Result<UnitComposition> Make(Unit unit)
        {
            composition ??= new(Units!);
            Units[0] = unit;
            ProcessUnits(composition);
            return Result.Create(composition, Messages);
        }

        protected override Unit? CompileNodes(DyCodeModel codeModel, bool root)
        {
            if (!root)
                return base.CompileNodes(codeModel, root);
            
            Messages.Clear();
            var oldCompiler = compiler;

            if (compiler is null)
                compiler = new(BuilderOptions, this);
            else
            {
                compiler = new(compiler);
                startOffset = composition!.Units[0].Ops.Count;
            }

            var res = compiler.Compile(codeModel);

            if (res.Messages.Any())
                Messages.AddRange(res.Messages);

            if (!res.Success)
            {
                compiler = oldCompiler;
                startOffset = null;
                return null;
            }

            if (startOffset is not null)
                res.Value!.Layouts[0].Address = startOffset.Value;

            return res.Value;
        }
    }
}
