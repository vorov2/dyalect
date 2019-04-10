using Dyalect.Compiler;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Linker
{
    public partial class DyLinker
    {
        private const string EXT = ".dy";
        private const string OBJ = ".dyo";
        private readonly FileLookup lookup;

        private readonly Dictionary<string, Unit> unitMap = new Dictionary<string, Unit>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, Type>> asmMap = new Dictionary<string, Dictionary<string, Type>>(StringComparer.OrdinalIgnoreCase);

        protected List<Unit> Units { get; } = new List<Unit>();

        protected List<BuildMessage> Messages { get; } = new List<BuildMessage>();

        public BuilderOptions Options { get; }

        public DyLinker(FileLookup lookup, BuilderOptions options)
        {
            this.lookup = lookup;
            this.Options = options;
            Units.Add(null);
        }

        protected internal virtual Result<Unit> Link(Reference mod)
        {
            Unit unit = null;

            if (mod.ModuleName == "lang")
                unit = new Lang();
            else if (mod.DllName != null)
                unit = LinkForeignModule(mod);
            else
            {
                var path = FindModule(mod.ModuleName, mod);

                if (path == null || unitMap.TryGetValue(path, out unit))
                    return Result.Create(unit, Messages);

                if (string.Equals(Path.GetExtension(path), EXT))
                    unit = ProcessSourceFile(path, mod);
                else
                    unit = ProcessObjectFile(path, mod);
            }

            if (unit != null && !unitMap.ContainsKey(unit.FileName))
            {
                unit.Id = Units.Count;
                Units.Add(unit);
                unitMap.Add(unit.FileName, unit);
            }

            var retval = Result.Create(unit, Messages);
            return retval;
        }

        public Result<UnitComposition> Make(SourceBuffer buffer)
        {
            var codeModel = ProcessBuffer(buffer);

            if (codeModel == null)
                return Result.Create(default(UnitComposition), Messages);

            return Make(codeModel);
        }

        public Result<UnitComposition> Make(DyCodeModel codeModel)
        {
            var unit = CompileNodes(codeModel, root: true);

            if (unit == null)
                return Result.Create(default(UnitComposition), Messages);

            return Make(unit);
        }

        protected virtual Result<UnitComposition> Make(Unit unit)
        {
            Units[0] = unit;
            var asm = new UnitComposition(Units);
            ProcessUnits(asm);
            return Result.Create(asm, Messages);
        }

        protected void ProcessUnits(UnitComposition asm)
        {
            foreach (var u in Units)
            {
                for (var i = 0; i < u.References.Count; i++)
                    u.ModuleHandles[i] = u.References[i].Id;

                for (var i = 0; i < u.TypeHandles.Count; i++)
                {
                    u.TypeHandles[i] = asm.Types.Count;
                    asm.Types.Add(new DyVariantTypeInfo(asm.Types.Count, u.TypeNames[i]));
                }
            }
        }

        private Unit ProcessSourceFile(string fileName, Reference reference)
        {
            string src;

            try
            {
                src = File.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                AddError(LinkerError.UnableReadModule, reference.SourceFileName, reference.SourceLocation, ex.Message);
                return null;
            }

            var codeModel = ProcessBuffer(new StringBuffer(src, fileName));
            return codeModel != null ? CompileNodes(codeModel, root: false) : null;
        }

        private DyCodeModel ProcessBuffer(SourceBuffer buffer)
        {
            var parser = new DyParser();
            var res = parser.Parse(buffer);

            if (!res.Success)
            {
                Messages.AddRange(res.Messages);
                return null;
            }

            return res.Value;
        }

        protected virtual Unit CompileNodes(DyCodeModel codeModel, bool root)
        {
            var compiler = new DyCompiler(Options, this);
            var res = compiler.Compile(codeModel);

            if (!res.Success)
            {
                Messages.AddRange(res.Messages);
                return null;
            }

            return res.Value;
        }

        private Unit ProcessObjectFile(string fileName, Reference reference)
        {
            throw new NotImplementedException();
        }

        private string FindModule(string module, Reference mod)
        {
            if (!module.EndsWith(EXT, StringComparison.OrdinalIgnoreCase))
                module += EXT;

            return FindModuleExact(module, mod);
        }

        private string FindModuleExact(string module, Reference mod)
        {
            if (lookup.Find(module, out var fullPath))
                return fullPath;

            AddError(LinkerError.ModuleNotFound, mod.SourceFileName, mod.SourceLocation, module);
            return null;
        }

        private void AddError(LinkerError error, string fileName, Location loc, params object[] args)
        {
            var str = LinkerErrors.ResourceManager.GetString(error.ToString());
            str = str ?? error.ToString();

            if (args != null)
                str = string.Format(str, args);

            Messages.Add(new BuildMessage(str, BuildMessageType.Error, (int)error, loc.Line, loc.Column, fileName));
        }
    }
}
