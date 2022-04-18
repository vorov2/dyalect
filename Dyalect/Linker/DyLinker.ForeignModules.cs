using Dyalect.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dyalect.Linker
{
    partial class DyLinker
    {
        private const string DYALECTLIB = "Dyalect.Library.dll";
        private Dictionary<string, ForeignUnit>? dyalectLib;

        private ForeignUnit? LinkForeignModule(Unit self, Reference mod)
        {
            var dict = LookupAssembly(self, mod.DllName!, mod);

            if (dict is not null)
            {
                if (!dict.TryGetValue(mod.ModuleName, out var unit))
                {
                    AddError(LinkerError.AssemblyModuleNotFound, mod.SourceFileName!, mod.SourceLocation,
                        mod.ModuleName, mod.DllName!);
                    return null;
                }

                return unit;
            }

            return null;
        }


        private Dictionary<string, ForeignUnit>? LookupAssembly(Unit self, string dll, Reference? @ref = null)
        {
            if (!AssemblyMap.TryGetValue(dll, out var dict))
            {
                if (!Lookup.Find(Path.GetDirectoryName(self.FileName), dll, out var path))
                    return null;

                dict = LoadAssembly(path, @ref ?? Reference.Empty);
                AssemblyMap.Add(dll, dict);

                if (dll == DYALECTLIB)
                    dyalectLib = dict;
            }

            return dict;
        }

        private Dictionary<string, ForeignUnit>? LoadAssembly(string path, Reference mod)
        {
            Assembly asm;

            try
            {
                asm = Assembly.LoadFrom(path);
            }
            catch (Exception ex)
            {
                AddError(LinkerError.UnableLoadAssembly, mod.SourceFileName!, mod.SourceLocation,
                    mod.DllName!, ex.Message);
                return null;
            }

            var dict = new Dictionary<string, ForeignUnit>();

            foreach (var t in asm.GetTypes())
            {
                if (Attribute.GetCustomAttribute(t, typeof(DyUnitAttribute)) is not DyUnitAttribute attr)
                    continue;
                
                if (dict.ContainsKey(attr.Name))
                    AddError(LinkerError.DuplicateModuleName, mod.SourceFileName!, mod.SourceLocation,
                        mod.DllName!, attr.Name);


                object module;

                try
                {
                    module = Activator.CreateInstance(t)!;
                }
                catch (Exception ex)
                {
                    AddError(LinkerError.AssemblyModuleLoadError, mod.SourceFileName!, mod.SourceLocation,
                        mod.ModuleName, mod.DllName!, ex.Message);
                    return null;
                }

                if (module is not ForeignUnit unit)
                {
                    AddError(LinkerError.InvalidAssemblyModule, mod.SourceFileName!, mod.SourceLocation,
                        mod.ModuleName, mod.DllName!);
                    return null;
                }

                unit.FileName = path;
                dict.Add(attr.Name, unit);
            }

            return dict;
        }
    }
}
