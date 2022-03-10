using Dyalect.Compiler;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dyalect.Linker
{
    public partial class DyLinker
    {
        private const string EXT = ".dy";
        private const string OBJ = ".dyo";
        private readonly Lang lang;

        protected Dictionary<Guid, Unit> UnitMap { get; set;  } = new();

        protected Dictionary<string, Dictionary<string, ForeignUnit>> AssemblyMap { get; set; } = 
            new Dictionary<string, Dictionary<string, ForeignUnit>>(StringComparer.OrdinalIgnoreCase);

        protected List<Unit> Units { get; set; } = new();

        protected List<BuildMessage> Messages { get; } = new List<BuildMessage>();

        public BuilderOptions BuilderOptions { get; }

        public FileLookup Lookup { get; }

        public DyLinker(FileLookup lookup, BuilderOptions options, DyTuple? args = null)
        {
            Lookup = lookup;
            BuilderOptions = options;
            lang = new(args) { FileName = nameof(lang), Id = 1 };
            Units.Add(null!);
        }

        protected internal virtual Result<Unit> Link(Unit self, Reference mod)
        {
            if (!UnitMap.TryGetValue(mod.Id, out var unit))
            {
                if (mod.LocalPath is null && mod.DllName is null && mod.ModuleName != nameof(lang))
                {
                    if (dyalectLib is null)
                        LookupAssembly(self, DYALECTLIB);

                    if (dyalectLib is not null && dyalectLib.ContainsKey(mod.ModuleName))
                        return Link(self, new Reference(mod.Id, mod.ModuleName, null, DYALECTLIB, mod.SourceLocation, mod.SourceFileName));
                }

                if (mod.ModuleName == nameof(lang))
                    unit = lang; 
                else if (mod.DllName is not null)
                {
                    unit = LinkForeignModule(self, mod);

                    if (unit is null)
                        AddError(LinkerError.AssemblyNotFound, mod.SourceFileName!, mod.SourceLocation, mod.DllName, mod.ModuleName);
                    else
                        foreach (var rf in unit.References)
                        {
                            var res = Link(self, rf);
                            if (!res.Success || res.Value is null)
                                return res;
                            rf.Instance = (ForeignUnit)res.Value;
                        }
                }
                else
                {
                    var path = FindModule(self, mod.GetPath(), mod);

                    if (path is not null && string.Equals(Path.GetExtension(path), EXT))
                        unit = ProcessSourceFile(path, mod);
                    else if (path is not null)
                        unit = ProcessObjectFile(path, mod);
                }

                if (unit is not null)
                {
                    unit.Id = Units.Count;
                    Units.Add(unit);
                    UnitMap.Add(mod.Id, unit);
                }
            }

            if (unit is not null && mod.Checksum != 0 && mod.Checksum != unit.Checksum && !BuilderOptions.LinkerSkipChecksum)
                AddError(LinkerError.ChecksumValidationFailed, mod.SourceFileName!, mod.SourceLocation, mod.ModuleName, unit.FileName ?? "<unknown>");

            return Result.Create(unit, Messages);
        }

        public Result<UnitComposition> Make(string filePath)
        {
            Messages.Clear();

            if (!Lookup.Find(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + OBJ, out var fullPath))
                if (!Lookup.Find(Path.GetDirectoryName(filePath), Path.GetFileName(filePath), out fullPath))
                {
                    AddError(LinkerError.ModuleNotFound, filePath, default, filePath);
                    return Result.Create(default(UnitComposition), Messages);
                }

            if (fullPath.EndsWith(OBJ, StringComparison.OrdinalIgnoreCase))
            {
                var unit = ProcessObjectFile(fullPath, default);

                if (unit is null)
                    return Result.Create(default(UnitComposition), Messages);

                return Make(unit);
            }

            SourceBuffer buffer;

            try
            {
                buffer = SourceBuffer.FromFile(fullPath);
            }
            catch (Exception ex)
            {
                AddError(LinkerError.UnableReadModule, fullPath, default, fullPath, ex.Message);
                return Result.Create(default(UnitComposition), Messages);
            }

            var codeModel = ProcessBuffer(buffer);

            if (codeModel is null)
                return Result.Create(default(UnitComposition), Messages);

            return Make(codeModel);
        }

        public Result<UnitComposition> Make(SourceBuffer buffer)
        {
            Messages.Clear();
            var codeModel = ProcessBuffer(buffer);

            if (codeModel == null)
                return Result.Create(default(UnitComposition), Messages);

            return Make(codeModel);
        }

        public Result<Unit> Compile(SourceBuffer buffer)
        {
            Messages.Clear();
            var codeModel = ProcessBuffer(buffer);

            if (codeModel is null)
                return Result.Create(default(Unit), Messages);

            return Compile(codeModel);
        }

        public Result<Unit> Compile(DyCodeModel codeModel)
        {
            Prepare();

            try
            {
                var unit = CompileNodes(codeModel, root: true);
                return Result.Create(unit, Messages);
            }
            finally
            {
                var failed = Messages.Any(m => m.Type == BuildMessageType.Error);
                Complete(failed);
            }
        }

        public Result<UnitComposition> Make(DyCodeModel codeModel)
        {
            Prepare();

            try
            {
                var unit = CompileNodes(codeModel, root: true);

                if (unit is null)
                    return Result.Create(default(UnitComposition), Messages);

                return Make(unit);
            }
            finally
            {
                var failed = Messages.Any(m => m.Type == BuildMessageType.Error);
                Complete(failed);
            }
        }

        protected virtual void Prepare() { }

        protected virtual void Complete(bool failed) { }

        protected virtual Result<UnitComposition> Make(Unit unit)
        {
            Units[0] = unit;
            var asm = new UnitComposition(Units!);
            ProcessUnits(asm);
            return Result.Create(asm, Messages);
        }

        protected void ProcessUnits(UnitComposition composition)
        {
            for (var uid = 0; uid < Units.Count; uid++)
            {
                var u = Units[uid];

                for (var i = 0; i < u!.References.Count; i++)
                {
                    var r = u.References[i];
                    u.UnitIds[i] = UnitMap[r.Id].Id;
                }

                if (u.References.Count > 0)
                    u.UnitIds[u.References.Count] = uid;
            }
        }

        private Unit? ProcessSourceFile(string fileName, Reference reference)
        {
            string src;

            try
            {
                src = File.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                AddError(LinkerError.UnableReadModule, reference.SourceFileName!, reference.SourceLocation, fileName, ex.Message);
                return null;
            }

            var codeModel = ProcessBuffer(new StringBuffer(src, fileName));
            return codeModel is not null ? CompileNodes(codeModel, root: false) : null;
        }

        private DyCodeModel? ProcessBuffer(SourceBuffer buffer)
        {
            var res = DyParser.Parse(buffer);

            if (!res.Success)
            {
                Messages.AddRange(res.Messages);
                return null;
            }

            return res.Value;
        }

        protected virtual Unit? CompileNodes(DyCodeModel codeModel, bool root)
        {
            var compiler = new DyCompiler(BuilderOptions, this);
            var res = compiler.Compile(codeModel);

            if (res.Messages.Any())
                Messages.AddRange(res.Messages);

            if (!res.Success)
                return null;

            return res.Value;
        }

        private Unit? ProcessObjectFile(string fileName, Reference? reference)
        {
#if !DEBUG
            try
#endif
            {
                var obj = ObjectFileReader.Read(fileName);

                foreach (var o in obj.References)
                    Link(obj, o);

                return obj;
            }
#if !DEBUG
            catch (Exception ex)
            {
                AddError(LinkerError.UnableReadObjectFile, fileName,
                    reference is not null ? reference.SourceLocation : default, fileName, ex.Message);
                return null;
            }
#endif
        }

        private string? FindModule(Unit self, string module, Reference mod)
        {
            var objModule = Path.Combine(Path.GetDirectoryName(module)!, Path.GetFileNameWithoutExtension(module) + OBJ);

            if (FindModuleExact(self.FileName!, objModule, mod, out var path))
                return path;

            if (!module.EndsWith(EXT, StringComparison.OrdinalIgnoreCase))
                module += EXT;

            if (!FindModuleExact(self.FileName!, module, mod, out path))
            {
                AddError(LinkerError.ModuleNotFound, mod.SourceFileName!, mod.SourceLocation, module);
                return null;
            }
            else
                return path;
        }

        private bool FindModuleExact(string workingDir, string module, Reference mod, out string? path)
        {
            path = null;

            if (!Lookup.Find(Path.GetDirectoryName(workingDir), module, out var fullPath))
                return false;
            
            if (NeedReport((int)LinkerWarning.NewerSourceFile)
                && !string.Equals(Path.GetExtension(module), ".DLL", StringComparison.OrdinalIgnoreCase))
            {
                var sf = Path.Combine(Path.GetDirectoryName(fullPath)!, Path.GetFileNameWithoutExtension(fullPath) + ".dy");

                if (File.Exists(sf) && File.GetLastWriteTime(sf) > File.GetLastWriteTime(fullPath))
                    AddWarning(LinkerWarning.NewerSourceFile, mod.SourceFileName!, mod.SourceLocation, Path.GetFileNameWithoutExtension(fullPath));
            }

            path = fullPath.Replace('\\', '/');
            return true;

        }

        private void AddError(LinkerError error, string fileName, Location loc, params object[] args) =>
            AddMessage(BuildMessageType.Error, (int)error, error.ToString(), fileName, loc, args);

        private void AddWarning(LinkerWarning warn, string fileName, Location loc, params object[] args) =>
            AddMessage(BuildMessageType.Warning, (int)warn, warn.ToString(), fileName, loc, args);

        private void AddMessage(BuildMessageType type, int code, string codeName, string fileName, Location loc, params object[] args)
        {
            if (type == BuildMessageType.Warning || NeedReport(code))
                return;

            var str = LinkerErrors.ResourceManager.GetString(codeName);
            str ??= codeName;

            if (args is not null)
                str = string.Format(str, args);

            Messages.Add(new(str, type, code, loc.Line, loc.Column, fileName));
        }

        private bool NeedReport(int warn) =>
            BuilderOptions.NoWarningsLinker || BuilderOptions.IgnoreWarnings.Contains(warn);
    }
}
