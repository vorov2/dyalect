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
        private readonly BuilderOptions options;

        private readonly Dictionary<string, Unit> unitMap = new Dictionary<string, Unit>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, Type>> asmMap = new Dictionary<string, Dictionary<string, Type>>(StringComparer.OrdinalIgnoreCase);
        private readonly List<Unit> units = new List<Unit>();
        private readonly List<BuildMessage> messages = new List<BuildMessage>();

        public DyLinker(FileLookup lookup, BuilderOptions options)
        {
            this.lookup = lookup;
            this.options = options;
            units.Add(null);
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
                    return Result.Create(unit, messages);

                if (string.Equals(Path.GetExtension(path), EXT))
                    unit = ProcessSourceFile(path, mod);
                else
                    unit = ProcessObjectFile(path, mod);
            }

            if (unit != null && !unitMap.ContainsKey(unit.FileName))
            {
                unit.Id = units.Count;
                units.Add(unit);
                unitMap.Add(unit.FileName, unit);
            }

            var retval = Result.Create(unit, messages);
            return retval;
        }

        public virtual Result<UnitComposition> Make(SourceBuffer buffer)
        {
            var unit = ProcessBuffer(buffer);

            if (unit == null)
                return Result.Create(default(UnitComposition), messages);

            return Make(unit);
        }

        public Result<UnitComposition> Make(DyCodeModel codeModel)
        {
            var unit = CompileNodes(codeModel);

            if (unit == null)
                return Result.Create(default(UnitComposition), messages);

            return Make(unit);
        }

        private Result<UnitComposition> Make(Unit unit)
        {
            units[0] = unit;
            var asm = new UnitComposition(units);
            var mixins = new Dictionary<string, object>();

            foreach (var u in units)
            {
                for (var i = 0; i < u.References.Count; i++)
                    u.ModuleHandles[i] = u.References[i].Id;

                for (var i = 0; i < u.TypeHandles.Count; i++)
                {
                    u.TypeHandles[i] = asm.Types.Count;
                    asm.Types.Add(new DyVariantTypeInfo(asm.Types.Count, u.TypeNames[i]));
                }
            }

            return Result.Create(asm, messages);
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

            return ProcessBuffer(new StringBuffer(src, fileName));
        }

        private Unit ProcessBuffer(SourceBuffer buffer)
        {
            var parser = new DyParser();
            var res = parser.Parse(buffer);

            if (!res.Success)
            {
                messages.AddRange(res.Messages);
                return null;
            }

            return CompileNodes(res.Value);
        }

        private Unit CompileNodes(DyCodeModel codeModel)
        {
            var compiler = new DyCompiler(options, this);
            var res = compiler.Compile(codeModel);

            if (!res.Success)
            {
                messages.AddRange(res.Messages);
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

            messages.Add(new BuildMessage(str, BuildMessageType.Error, (int)error, loc.Line, loc.Column, fileName));
        }
    }
}
