using Dyalect.Compiler;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dyalect.Linker
{
    partial class DyLinker
    {
        private Unit LinkForeignModule(Unit self, Reference mod, string alt = null)
        {
            var dll = alt ?? mod.DllName;

            if (!FindModuleExact(self.FileName, dll, mod, out var path))
                return null;

            if (!AssemblyMap.TryGetValue(path, out Dictionary<string, Type> dict))
                dict = LoadAssembly(path, mod);

            if (dict != null)
            {
                if (!dict.TryGetValue(mod.ModuleName, out Type sysType))
                {
                    AddError(LinkerError.AssemblyModuleNotFound, mod.SourceFileName, mod.SourceLocation,
                        mod.ModuleName, mod.DllName);
                    return null;
                }

                object module;

                try
                {
                    module = Activator.CreateInstance(sysType);
                }
                catch (Exception ex)
                {
                    AddError(LinkerError.AssemblyModuleLoadError, mod.SourceFileName, mod.SourceLocation,
                        mod.ModuleName, mod.DllName, ex.Message);
                    return null;
                }

                if (!(module is Unit unit))
                {
                    AddError(LinkerError.InvalidAssemblyModule, mod.SourceFileName, mod.SourceLocation,
                        mod.ModuleName, mod.DllName);
                    return null;
                }

                unit.FileName = path;
                return unit;
            }

            return null;
        }

        private Dictionary<string, Type> LoadAssembly(string path, Reference mod)
        {
            Assembly asm;

            try
            {
                asm = Assembly.LoadFrom(path);
            }
            catch (Exception ex)
            {
                AddError(LinkerError.UnableLoadAssembly, mod.SourceFileName, mod.SourceLocation,
                    mod.DllName, ex.Message);
                return null;
            }

            var dict = new Dictionary<string, Type>();

            foreach (var t in asm.GetTypes())
            {
                if (Attribute.GetCustomAttribute(t, typeof(DyUnitAttribute)) is DyUnitAttribute attr)
                {
                    if (dict.ContainsKey(attr.Name))
                        AddError(LinkerError.DuplicateModuleName, mod.SourceFileName, mod.SourceLocation,
                            mod.DllName, attr.Name);
                    else
                        dict.Add(attr.Name, t);
                }
            }

            AssemblyMap.Add(path, dict);
            return dict;
        }
    }
}
