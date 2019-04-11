using Dyalect.Compiler;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dyalect.Linker
{
    partial class DyLinker
    {
        private Unit LinkForeignModule(Reference mod)
        {
            var path = FindModuleExact(mod.DllName + ".dll", mod);

            if (path == null)
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
                var attr = Attribute.GetCustomAttribute(t, typeof(DyModuleAttribute))
                    as DyModuleAttribute;

                if (attr != null)
                {
                    if (dict.ContainsKey(attr.ModuleName))
                        AddError(LinkerError.DuplicateModuleName, mod.SourceFileName, mod.SourceLocation,
                            mod.DllName, attr.ModuleName);
                    else
                        dict.Add(attr.ModuleName, t);
                }
            }

            AssemblyMap.Add(path, dict);
            return dict;
        }
    }
}
