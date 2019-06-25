using Dyalect.Compiler;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dyalect.Linker
{
    public partial class DyLinker
    {
        private const string EXT = ".dy";
        private const string OBJ = ".dyo";
        private static Lang lang;
        private static readonly object syncRoot = new object();

        protected Dictionary<string, Unit> UnitMap { get; set;  } = new Dictionary<string, Unit>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, Dictionary<string, Type>> AssemblyMap { get; set; } = new Dictionary<string, Dictionary<string, Type>>(StringComparer.OrdinalIgnoreCase);

        protected List<Unit> Units { get; set; } = new List<Unit>();

        protected List<BuildMessage> Messages { get; } = new List<BuildMessage>();

        public BuilderOptions BuilderOptions { get; }

        public FileLookup Lookup { get; }

        public DyLinker(FileLookup lookup, BuilderOptions options)
        {
            this.Lookup = lookup;
            this.BuilderOptions = options;
            Units.Add(null);
        }

        protected internal virtual Result<Unit> Link(Reference mod)
        {
            Unit unit = null;

            if (mod.ModuleName == nameof(lang))
            {
                if (lang == null)
                    lock (syncRoot)
                        if (lang == null)
                            lang = new Lang();
                unit = lang;
                unit.FileName = nameof(lang);
            }
            else if (mod.DllName != null)
                unit = LinkForeignModule(mod);
            else
            {
                var path = FindModule(mod.GetPath(), mod);

                if (path == null || UnitMap.TryGetValue(path, out unit))
                    return Result.Create(unit, Messages);

                if (string.Equals(Path.GetExtension(path), EXT))
                    unit = ProcessSourceFile(path, mod);
                else
                    unit = ProcessObjectFile(path, mod);
            }

            if (unit != null && !UnitMap.ContainsKey(unit.FileName))
            {
                unit.Id = Units.Count;
                Units.Add(unit);
                UnitMap.Add(unit.FileName, unit);
            }

            var retval = Result.Create(unit, Messages);
            return retval;
        }

        public Result<UnitComposition> Make(SourceBuffer buffer)
        {
            Messages.Clear();
            var codeModel = ProcessBuffer(buffer);

            if (codeModel == null)
                return Result.Create(default(UnitComposition), Messages);

            return Make(codeModel);
        }

        public Result<UnitComposition> Make(DyCodeModel codeModel)
        {
            Prepare();

            try
            {
                var unit = CompileNodes(codeModel, root: true);

                if (unit == null)
                    return Result.Create(default(UnitComposition), Messages);

                return Make(unit);
            }
            finally
            {
                var failed = Messages.Any(m => m.Type == BuildMessageType.Error);
                Complete(failed);
            }
        }

        protected virtual void Prepare()
        {
        }

        protected virtual void Complete(bool failed)
        {
        }

        protected virtual Result<UnitComposition> Make(Unit unit)
        {
            Units[0] = unit;
            var asm = new UnitComposition(Units);
            ProcessUnits(asm);
            return Result.Create(asm, Messages);
        }

        protected void ProcessUnits(UnitComposition composition)
        {
            foreach (var u in Units)
            {
                for (var i = 0; i < u.References.Count; i++)
                    u.UnitIds[i] = u.References[i].Id;

                for (var i = 0; i < u.MemberNames.Count; i++)
                {
                    if (!composition.MembersMap.TryGetValue(u.MemberNames[i], out var id))
                    {
                        id = composition.Members.Count;
                        composition.Members.Add(u.MemberNames[i]);
                        composition.MembersMap.Add(u.MemberNames[i], id);
                    }

                    u.MemberIds[i] = id;
                }

                for (var i = 0; i < u.Types.Count; i++)
                {
                    var td = u.Types[i];
                    td.Id = composition.Types.Count;
                    composition.Types.Add(new DyCustomTypeInfo(composition.Types.Count, td.Name, td.AutoGenConstructors));
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
                AddError(LinkerError.UnableReadModule, reference.SourceFileName, reference.SourceLocation, fileName, ex.Message);
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
            var compiler = new DyCompiler(BuilderOptions, this);
            var res = compiler.Compile(codeModel);

            if (res.Messages.Any())
                Messages.AddRange(res.Messages);

            if (!res.Success)
                return null;

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
            if (Lookup.Find(module, out var fullPath))
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
